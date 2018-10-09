using Infraestructura.Crosscutting.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Infraestructura.Crosscutting.Network.Identity
{
    public class ADOIdentityGenerator : IIdentityGenerator
    {
        private const string ConnectionString = "connectionString";
        private readonly string _connectionExceptionMessage = string.Format("exception_ConnectionStringNotFound", ConnectionString);
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings[ConnectionString].ToString();

        #region Implementation of IIdentityGenerator

        /// <summary>
        /// Generates a new secuential transaction identities with GUIDs across system boundaries, ideal for databases.
        /// </summary>
        /// <returns>The new Transaction identity.</returns>
        public TransactionIdentity NewSequentialTransactionIdentity()
        {
            return new TransactionIdentity
            {
                TransactionId = NewSequentialGuid(),
                TransactionDate = DateTime.Now,
            };
        }

        /// <summary>
        /// Generates the next correlative indentity of a speccific correlative id.
        /// </summary>
        /// <param name="correlativeId">Correlative Id used to calculate the next Identity.</param>
        /// <returns>The next correlative identity.</returns>
        public string NextCorrelativeIdentity(string facilityId, string correlativeId)
        {
            string siguienteCorrelativo;
            if (string.IsNullOrWhiteSpace(_connectionString)) throw new ApplicationException(_connectionExceptionMessage);

            using (var conexionSp = new SqlConnection(_connectionString))
            {
                using (var comandoSp = new SqlCommand("sp_nextnumber", conexionSp))
                {
                    comandoSp.CommandType = CommandType.StoredProcedure;
                    comandoSp.Parameters.AddWithValue("@ID", correlativeId).Direction = ParameterDirection.Input;
                    comandoSp.Parameters.Add("@NextNumber", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;

                    conexionSp.Open();

                    SqlDataReader dr = comandoSp.ExecuteReader();
                    dr.Close();

                    siguienteCorrelativo = comandoSp.Parameters["@NextNumber"].Value.ToString().Trim();
                    conexionSp.Close();
                }
            }

            if (string.IsNullOrWhiteSpace(siguienteCorrelativo))
            {
                throw new ApplicationException(string.Format("exception_UnabledToGenerateCorrelative"));
            }

            return siguienteCorrelativo.Trim();
        }

        /// <inheritdoc/>
        public bool CorrelativeConfigurationExists(string facilityId, string correlativeId)
        {
            if (string.IsNullOrWhiteSpace(facilityId)) return false;
            if (string.IsNullOrWhiteSpace(correlativeId)) return false;
            if (string.IsNullOrWhiteSpace(_connectionString)) return false;

            int count = 0;

            using (var conexionSp = new SqlConnection(_connectionString))
            {
                var query = "SELECT COUNT(PaqueteId) FROM Comunes.CorrelativosPaquetes " +
                            "WHERE PaqueteId = @correlativeId AND PlantaId = @facilityId";

                using (var comandoSp = new SqlCommand(query, conexionSp))
                {
                    comandoSp.CommandType = CommandType.Text;
                    comandoSp.Parameters.AddWithValue("@correlativeId", correlativeId).Direction = ParameterDirection.Input;
                    comandoSp.Parameters.AddWithValue("@facilityId", facilityId).Direction = ParameterDirection.Input;

                    conexionSp.Open();

                    count = comandoSp.ExecuteScalar() is int ? (int)comandoSp.ExecuteScalar() : 0;
                    conexionSp.Close();
                }
            }
            return count > 0;
        }

        /// <summary>
        /// Generates the next correlative indentity of a speccific correlative id.
        /// </summary>
        /// <param name="facilityId">Facility Id to wich the correlative id belongs.</param>
        /// <param name="correlativeId">Correlative Id used to calculate the next Identity.</param>
        /// <returns>The next correlative identity.</returns>
        public string NextCorrelativeIdentity(string correlativeId)
        {
            string siguienteCorrelativo;
            if (string.IsNullOrWhiteSpace(_connectionString)) throw new ApplicationException(_connectionExceptionMessage);

            using (var conexionSp = new SqlConnection(_connectionString))
            {
                using (var comandoSp = new SqlCommand("Comunes.ObtenerCorrelativo", conexionSp))
                {
                    comandoSp.CommandType = CommandType.StoredProcedure;
                    comandoSp.Parameters.AddWithValue("@PaqueteTransID", correlativeId).Direction = ParameterDirection.Input;
                    comandoSp.Parameters.Add("@SiguienteCorrelativo", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;

                    conexionSp.Open();

                    var dr = comandoSp.ExecuteReader();
                    dr.Close();

                    siguienteCorrelativo = comandoSp.Parameters["@SiguienteCorrelativo"].Value.ToString().Trim();
                    conexionSp.Close();
                }
            }

            if (string.IsNullOrWhiteSpace(siguienteCorrelativo))
            {
                throw new ApplicationException(string.Format("exception_UnabledToGenerateCorrelative"));
            }

            return siguienteCorrelativo.Trim();
        }

        public List<string> ListCorrelativeIdentity(string facilityId, string correlativeId, int numberCorrelative)
        {
            var siguientesCorrelativo = new List<string>();
            if (string.IsNullOrWhiteSpace(_connectionString)) throw new ApplicationException(_connectionExceptionMessage);

            using (var conexionSp = new SqlConnection(_connectionString))
            {
                using (var comandoSp = new SqlCommand("Comunes.ObtenerCorrelativos", conexionSp))
                {
                    comandoSp.CommandType = CommandType.StoredProcedure;
                    comandoSp.Parameters.AddWithValue("@PlantaID", facilityId).Direction = ParameterDirection.Input;
                    comandoSp.Parameters.AddWithValue("@PaqueteTransID", correlativeId).Direction = ParameterDirection.Input;
                    comandoSp.Parameters.AddWithValue("@CantidadCorrelativos", numberCorrelative).Direction = ParameterDirection.Input;

                    conexionSp.Open();

                    var dr = comandoSp.ExecuteReader();
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            siguientesCorrelativo.Add(dr[0].ToString().Trim());
                        }
                    }

                    dr.Close();
                    conexionSp.Close();
                }
            }

            if (!siguientesCorrelativo.Any())
            {
                throw new ApplicationException(string.Format("exception_UnabledToGenerateCorrelative"));
            }

            return siguientesCorrelativo;
        }

        /// <summary>
        /// Generates a new secuential indentity for a Batch.
        /// The generated Id is in the following format: facilityId + year + week/>
        /// </summary>
        /// <param name="facilityId">The facility Id where for the new batch identity.</param>
        /// <param name="year">Year segment of the new batch identity</param>
        /// <param name="week">Week segment ofthe new batch identity.</param>
        /// <returns>The new Batch's sequential identity.</returns>
        public string NewSequentialBatchIdentity(string facilityId, string year, string week)
        {
            #region Validating arguments

            if (string.IsNullOrWhiteSpace(facilityId)) throw new ArgumentNullException("facilityId");
            if (string.IsNullOrWhiteSpace(year)) throw new ArgumentNullException("year");
            //if(string.IsNullOrWhiteSpace(week)) throw new ArgumentNullException("week");
            if (year.Trim().Length != 2) throw new ArgumentOutOfRangeException("year");
            //if (week.Trim().Length != 2) throw new ArgumentOutOfRangeException("week");

            #endregion Validating arguments

            var cal = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay,
                                                                        DayOfWeek.Sunday);
            cal = cal != 15 ? cal : (cal + 1);

            var paqueteBatch = cal.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0').Substring(0, 2);

            VerificarExistenciaCorrelativo(facilityId, paqueteBatch);

            var correlativo = NextCorrelativeIdentity(facilityId, paqueteBatch);

            return correlativo.Trim();
        }

        private void VerificarExistenciaCorrelativo(string planta, string paquete)
        {
            if (string.IsNullOrWhiteSpace(_connectionString)) throw new ApplicationException(_connectionExceptionMessage);

            using (var conexionSp = new SqlConnection(_connectionString))
            {
                using (var comandoSp = new SqlCommand("Comunes.VerificarExistenciaCorrelativoBatch", conexionSp))
                {
                    comandoSp.CommandType = CommandType.StoredProcedure;
                    comandoSp.Parameters.AddWithValue("@PlantaID", planta).Direction = ParameterDirection.Input;
                    comandoSp.Parameters.AddWithValue("@PaqueteID", paquete).Direction = ParameterDirection.Input;

                    conexionSp.Open();

                    var dr = comandoSp.ExecuteReader();
                    dr.Close();
                    conexionSp.Close();
                }
            }
        }

        #endregion Implementation of IIdentityGenerator

        #region Private Methods

        /// <summary>
        /// This algorithm generates secuential GUIDs across system boundaries, ideal for databases
        /// </summary>
        /// <returns></returns>
        public static Guid NewSequentialGuid()
        {
            byte[] uid = Guid.NewGuid().ToByteArray();
            byte[] binDate = BitConverter.GetBytes(DateTime.UtcNow.Ticks);

            var secuentialGuid = new byte[uid.Length];

            secuentialGuid[0] = uid[0];
            secuentialGuid[1] = uid[1];
            secuentialGuid[2] = uid[2];
            secuentialGuid[3] = uid[3];
            secuentialGuid[4] = uid[4];
            secuentialGuid[5] = uid[5];
            secuentialGuid[6] = uid[6];
            // set the first part of the 8th byte to '1100' so
            // later we'll be able to validate it was generated by us

            secuentialGuid[7] = (byte)(0xc0 | (0xf & uid[7]));

            // the last 8 bytes are sequential,
            // it minimizes index fragmentation
            // to a degree as long as there are not a large
            // number of Secuential-Guids generated per millisecond

            secuentialGuid[9] = binDate[0];
            secuentialGuid[8] = binDate[1];
            secuentialGuid[15] = binDate[2];
            secuentialGuid[14] = binDate[3];
            secuentialGuid[13] = binDate[4];
            secuentialGuid[12] = binDate[5];
            secuentialGuid[11] = binDate[6];
            secuentialGuid[10] = binDate[7];

            return new Guid(secuentialGuid);
        }

        #endregion Private Methods
    }
}
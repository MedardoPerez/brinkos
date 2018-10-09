using Infraestructura.Crosscutting.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;

namespace Infraestructura.Crosscutting.Network.Logging
{
    public sealed class TraceSourceLog : ILogger
    {
        private readonly TraceSource _source;

        /// <summary>
        /// Create a new instance of this trace manager
        /// </summary>
        public TraceSourceLog()
        {
            // Create default source
            _source = new TraceSource("CTS.NET");
        }

        /// <inheritdoc />
        public void LogInfo(string message, params object[] args)
        {
            if (!String.IsNullOrWhiteSpace(message))
            {
                var messageToTrace = string.Format(CultureInfo.InvariantCulture, message, args);

                TraceInternal(TraceEventType.Information, messageToTrace);
            }
        }

        /// <inheritdoc />
        public void LogWarning(string message, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var messageToTrace = string.Format(CultureInfo.InvariantCulture, message, args);

                TraceInternal(TraceEventType.Warning, messageToTrace);
            }
        }

        /// <inheritdoc />
        public void LogError(string message, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var messageToTrace = (args != null && args.Length > 0)
                    ? string.Format(CultureInfo.InvariantCulture, message, args)
                    : message;

                TraceInternal(TraceEventType.Error, messageToTrace);
            }
        }

        /// <inheritdoc />
        public void LogException(string moduleName, string typeFullName, string methodName, int lineNumber,
            string exceptionType, string exceptionMessage, string message)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["connectionString"];
                if (connectionString != null)
                {
                    var connection = new SqlConnection(connectionString.ConnectionString);
                    const string sql = @"insert into Comunes.LogEventosExcepciones
                                (EventType, Server, ModuleName, TypeFullName, MethodName,
                                LineNumber, ExceptionType, ExceptionMessage, Message, Date)
                                values(@EventTYpe, @Server, @ModuleName, @TypeFullName, @MethodName,
                                        @LineNumber, @ExceptionType, @ExceptionMessage, @Message, @Date)";

                    using (connection)
                    {
                        var command = new SqlCommand(sql, connection);
                        command.Parameters.AddWithValue("@EventTYpe", TraceEventType.Error);
                        command.Parameters.AddWithValue("@Server", Environment.MachineName);
                        command.Parameters.AddWithValue("@ModuleName", moduleName);
                        command.Parameters.AddWithValue("@TypeFullName", typeFullName);
                        command.Parameters.AddWithValue("@MethodName", methodName);
                        command.Parameters.AddWithValue("@LineNumber", lineNumber);
                        command.Parameters.AddWithValue("@ExceptionType", exceptionType);
                        command.Parameters.AddWithValue("@ExceptionMessage", exceptionMessage);
                        command.Parameters.AddWithValue("@Message", message);
                        command.Parameters.AddWithValue("@Date", DateTime.Now);
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }
            catch (Exception)
            {
                //Cannot access to file listener or cannot have
                //privileges to write in event log etc...
            }
        }

        /// <inheritdoc />
        public void LogTransaction(Transaction transaction)
        {
            SqlTransaction sqlTransaction = null;
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["connectionString"];
                if (connectionString != null)
                {
                    string sql = "insert into  Comunes.LogTransacciones(TransaccionUId, TipoTransaccion, FechaTransaccion, FechaTransaccionUtc, " +
                        "ModificadoPor, OrigenTransaccion) " +
                                       "values(@TransaccionUId, @TipoTransaccion, @FechaTransaccion, @FechaTransaccionUtc, " +
                        "@ModificadoPor, @OrigenTransaccion)";

                    using (var connection = new SqlConnection(connectionString.ConnectionString))
                    {
                        connection.Open();
                        sqlTransaction = connection.BeginTransaction();

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Transaction = sqlTransaction;

                            // Inserting Transaction Header
                            command.Parameters.AddWithValue("@TransaccionUId", transaction.TransactionId);
                            command.Parameters.AddWithValue("@TipoTransaccion", transaction.TransactionType);
                            command.Parameters.AddWithValue("@FechaTransaccion", transaction.TransactionDate);
                            command.Parameters.AddWithValue("@ModificadoPor", transaction.ModifiedBy);
                            command.Parameters.AddWithValue("@OrigenTransaccion", transaction.TransactionOrigen);

                            command.ExecuteNonQuery();

                            command.Parameters.Clear();

                            command.Parameters.Add("@TransaccionUId", SqlDbType.UniqueIdentifier);
                            command.Parameters.Add("@EntidadDominio", SqlDbType.VarChar);
                            command.Parameters.Add("@DescripcionTransaccion", SqlDbType.VarChar);

                            sql = "insert into  Comunes.LogTransaccionesDetalle(TransaccionUId, EntidadDominio, DescripcionTransaccion) " +
                                       "values(@TransaccionUId, @EntidadDominio, @DescripcionTransaccion)";

                            command.CommandText = sql;

                            foreach (var transactionDetail in transaction.TransactionDetail)
                            {
                                command.Parameters["@TransaccionUId"].Value = transactionDetail.TransactionId;
                                command.Parameters["@EntidadDominio"].Value = transactionDetail.TableName;
                                command.Parameters["@DescripcionTransaccion"].Value = transactionDetail.CrudOperation;

                                command.ExecuteNonQuery();
                            }
                        }

                        sqlTransaction.Commit();
                    }
                }
            }
            catch (SecurityException)
            {
                //Cannot access to file listener or cannot have
                //privileges to write in event log etc...
                if (sqlTransaction != null) sqlTransaction.Rollback();
                throw;
            }
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var messageToTrace = string.Format(CultureInfo.InvariantCulture, message, args);

                if (_source != null)
                {
                    try
                    {
                        _source.TraceEvent(TraceEventType.Verbose, (int)TraceEventType.Verbose, messageToTrace);
                    }
                    catch (SecurityException)
                    {
                        //Cannot access to file listener or cannot have
                        //privileges to write in event log etc...
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Debug(string message, Exception exception, params object[] args)
        {
            if (!String.IsNullOrWhiteSpace(message)
                &&
                exception != null)
            {
                var messageToTrace = string.Format(CultureInfo.InvariantCulture, message, args);

                var exceptionData = exception.ToString(); // The ToString() create a string representation of the current exception

                TraceInternal(TraceEventType.Error, string.Format(CultureInfo.InvariantCulture, "{0} Exception:{1}", messageToTrace, exceptionData));
            }
        }

        /// <inheritdoc />
        public void Debug(object item)
        {
            if (item != null)
            {
                TraceInternal(TraceEventType.Verbose, item.ToString());
            }
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var messageToTrace = string.Format(CultureInfo.InvariantCulture, message, args);

                TraceInternal(TraceEventType.Critical, messageToTrace);
            }
        }

        /// <inheritdoc />
        public void Fatal(string message, Exception exception, params object[] args)
        {
            if (!string.IsNullOrWhiteSpace(message)
                &&
                exception != null)
            {
                var messageToTrace = string.Format(CultureInfo.InvariantCulture, message, args);

                var exceptionData = exception.ToString(); // The ToString() create a string representation of the current exception

                TraceInternal(TraceEventType.Critical, string.Format(CultureInfo.InvariantCulture, "{0} Exception:{1}", messageToTrace, exceptionData));
            }
        }

        /// <summary>
        /// Trace internal message in configured listeners
        /// </summary>
        /// <param name="eventType">Event type to trace</param>
        /// <param name="message">Message of event</param>
        private void TraceInternal(TraceEventType eventType, string message)
        {
            if (_source != null)
            {
                try
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["connectionString"];
                    if (connectionString != null)
                    {
                        var moduleName = ExtractModuleNameFromMessage(message);

                        var connection = new SqlConnection(connectionString.ConnectionString);
                        const string sql = "insert into Comunes.LogEventosExcepciones(EventType, ModuleName, Message, Date) " +
                                           "values(@EventTYpe, @ModuleName, @Message, @Date)";

                        using (connection)
                        {
                            var command = new SqlCommand(sql, connection);
                            command.Parameters.AddWithValue("@EventTYpe", eventType);
                            command.Parameters.AddWithValue("@ModuleName", moduleName);
                            command.Parameters.AddWithValue("@Message", message);
                            command.Parameters.AddWithValue("@Date", DateTime.Now);
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }

                    _source.TraceEvent(eventType, (int)eventType, message);
                }
                catch (SecurityException)
                {
                    //Cannot access to file listener or cannot have
                    //privileges to write in event log etc...
                }
            }
        }

        /// <summary>
        /// Trace internal message in configured listeners
        /// </summary>
        /// <param name="eventType">Event type to trace</param>
        /// <param name="message">Message of event</param>
        private void TraceInternal(TraceEventType eventType, Exception exception, string message)
        {
            if (_source != null)
            {
                try
                {
                    string exceptionType = exception != null ? exception.GetType().Name : null;
                    string exceptionMessage = exception.Message;

                    StackFrame stackFrame = GetCTSExceptionStackFrame(exception);
                    string typeFullName = stackFrame.GetMethod().ReflectedType.FullName;
                    string moduleName = ExtractModuleNameFromMessage(typeFullName);
                    string methodName = stackFrame.GetMethod().Name;
                    int lineNumber = stackFrame.GetFileLineNumber();

                    LogException(moduleName, typeFullName, methodName, lineNumber, exceptionType, exceptionMessage, message);

                    _source.TraceEvent(eventType, (int)eventType, message);
                }
                catch (SecurityException)
                {
                    //Cannot access to file listener or cannot have
                    //privileges to write in event log etc...
                }
            }
        }

        private StackFrame GetCTSExceptionStackFrame(Exception exception)
        {
            var ctsNameSpaces = GetCTSNameSpaces();

            StackTrace trace = new StackTrace(exception, true);
            var frames = trace.GetFrames();

            if (frames == null)
            {
                return new StackFrame();
            }
            int numero = 0;
            foreach (var frame in frames)
            {
                numero++;
                string typeFullName = frame.GetMethod().ReflectedType.FullName;
                string methodName = frame.GetMethod().Name;

                if (ctsNameSpaces.Any(n => typeFullName.Contains(n)))
                {
                    // We ignore the commit method on repositories,
                    // we are insterested in find the application service that invoked the commit operation
                    if (typeFullName.Contains("Infrastructure.Data.BoundedContext") && methodName.Contains("Commit"))
                    {
                        continue;
                    }

                    return frame;
                }
            }

            // If no frame relate to a CTS.NET type is found then we take the first stack frame.
            return frames[0];
        }

        private IEnumerable<string> GetCTSNameSpaces()
        {
            return new List<string>
            {
                "TestConfi",
                "Servicios",
                "API.DTO",
                "Dominio",
                "Infraestructura.Contexto",
                "Infraestructura.Crosscutting.Netframework",
                "Infraestructura.CrossCutting",
                "Framework.Core",
            };
        }

        private string ExtractModuleNameFromMessage(string errorMessage)
        {
            string[] lines = errorMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var words = line.Split('.');
                if (words.Length > 4 && words[0].Contains("Test") && words[1].Contains("Test"))
                {
                    return words[4];
                }
            }

            return "UnKnow Module";
        }
    }
}
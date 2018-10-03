using System.Collections.Generic;

namespace Infraestructura.Crosscutting.Identity
{
    public interface IIdentityGenerator
    {
        /// <summary>
        /// Generates a new secuential transaction identities with GUIDs across system boundaries, ideal for databases. 
        /// </summary>
        /// <returns></returns>
        TransactionIdentity NewSequentialTransactionIdentity();

        /// <summary>
        /// Generates the next correlative indentity of a speccific correlative id.
        /// </summary>
        /// <param name="correlativeId">Correlative Id used to calculate the next Identity.</param>
        /// <returns>The next correlative identity.</returns>
        string NextCorrelativeIdentity(string correlativeId);

        bool CorrelativeConfigurationExists(string facilityId, string correlativeId);

        /// <summary>
        /// Generates the next correlative indentity of a speccific correlative id.
        /// </summary>
        /// <param name="facilityId">Facility Id to wich the correlative id belongs.</param>
        /// <param name="correlativeId">Correlative Id used to calculate the next Identity.</param>
        /// <returns>The next correlative identity.</returns>
        string NextCorrelativeIdentity(string facilityId, string correlativeId);

        /// <summary>
        /// Generates the list correlative indentity of a speccific correlative id.
        /// </summary>
        /// <param name="facilityId">Facility Id to wich the correlative id belongs.</param>
        /// <param name="correlativeId">Correlative Id used to calculate the next Identity.</param>
        /// <param name="numberCorrelative">Number of Correlative to create</param>
        /// <returns>The list correlative identity.</returns>
        List<string> ListCorrelativeIdentity(string facilityId, string correlativeId, int numberCorrelative);

        /// <summary>
        /// Generates a new secuential indentity for a Batch.
        /// The generated Id is in the following format: facilityId + year + week/>
        /// </summary>
        /// <param name="facilityId">The facility Id where for the new batch identity.</param>
        /// <param name="year">Year segment of the new batch identity, must be a 2 characters string.</param>
        /// <param name="week">Week segment ofthe new batch identity, must be a 2 characters string.</param>
        /// <returns>The new Batch's sequential identity.</returns>
        string NewSequentialBatchIdentity(string facilityId, string year, string week);
    }
}

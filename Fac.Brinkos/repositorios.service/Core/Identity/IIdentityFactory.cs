namespace Infraestructura.Crosscutting.Identity
{
    public interface IIdentityFactory
    {
        /// <summary>
        /// Create a new IIdentityGenerator.
        /// </summary>
        /// <returns>The IIdentityGenerator created.</returns>
        IIdentityGenerator Create();
    }
}

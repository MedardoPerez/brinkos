namespace Infraestructura.Crosscutting.Identity
{
    public static class IdentityFactory
    {
        private static IIdentityFactory _identityFactory;

        /// <summary>
        /// Set the  identity factory to use
        /// </summary>
        /// <param name="identityFactory">Log factory to use</param>
        public static void SetCurrent(IIdentityFactory identityFactory)
        {
            _identityFactory = identityFactory;
        }

        /// <summary>
        /// Createt a new IIdentityGenerator
        /// <paramref>
        ///     <name>CTS.NET.Infrastructure.Crosscutting.Identity.IIdentityGenerator</name>
        /// </paramref>
        /// </summary>
        /// <returns>Created IIdentityGenerator</returns>
        public static IIdentityGenerator CreateIdentity()
        {
            return (_identityFactory != null) ? _identityFactory.Create() : null;
        }
    }
}
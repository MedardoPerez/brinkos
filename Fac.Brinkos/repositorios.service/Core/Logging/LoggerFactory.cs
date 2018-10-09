namespace Infraestructura.Crosscutting.Logging
{
    public static class LoggerFactory
    {
        #region Members

        static ILoggerFactory _currentLogFactory;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the  log factory to use
        /// </summary>
        /// <param name="logFactory">Log factory to use</param>
        public static void SetCurrent(ILoggerFactory logFactory)
        {
            _currentLogFactory = logFactory;
        }

        /// <summary>
        /// Createt a new 
        /// <paramref>
        ///     <name>CTS.NET.Infrastructure.Crosscutting.Logging.ILogger</name>
        /// </paramref>
        /// </summary>
        /// <returns>Created ILog</returns>
        public static ILogger CreateLog()
        {
            return (_currentLogFactory != null) ? _currentLogFactory.Create() : null;
        }

        #endregion
    }
}

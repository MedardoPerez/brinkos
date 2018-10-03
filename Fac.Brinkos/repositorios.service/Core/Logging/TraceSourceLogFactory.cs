using Infraestructura.Crosscutting.Logging;

namespace Infraestructura.Crosscutting.Network.Logging
{
    public class TraceSourceLogFactory : ILoggerFactory
    {
        /// <summary>
        /// Create the trace source log
        /// </summary>
        /// <returns>New ILog based on Trace Source infrastructure</returns>
        public ILogger Create()
        {
            return new TraceSourceLog();
        }
    }
}

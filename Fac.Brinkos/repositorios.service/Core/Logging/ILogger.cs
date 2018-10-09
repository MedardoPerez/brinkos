using System;

namespace Infraestructura.Crosscutting.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Log debug message
        /// </summary>
        /// <param name="message">The debug message</param>
        /// <param name="args">the message argument values</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Log debug message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="exception">Exception to write in debug message</param>
        /// <param name="args"> </param>
        void Debug(string message, Exception exception, params object[] args);

        /// <summary>
        /// Log debug message
        /// </summary>
        /// <param name="item">The item with information to write in debug</param>
        void Debug(object item);

        /// <summary>
        /// Log FATAL error
        /// </summary>
        /// <param name="message">The message of fatal error</param>
        /// <param name="args">The argument values of message</param>
        void Fatal(string message, params object[] args);

        /// <summary>
        /// log FATAL error
        /// </summary>
        /// <param name="message">The message of fatal error</param>
        /// <param name="exception">The exception to write in this fatal message</param>
        /// <param name="args"> </param>
        void Fatal(string message, Exception exception, params object[] args);

        /// <summary>
        /// Log message information
        /// </summary>
        /// <param name="message">The information message to write</param>
        /// <param name="args">The arguments values</param>
        void LogInfo(string message, params object[] args);

        /// <summary>
        /// Log warning message
        /// </summary>
        /// <param name="message">The warning message to write</param>
        /// <param name="args">The argument values</param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">The error message to write</param>
        /// <param name="args">The arguments values</param>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Log transaction.
        /// </summary>
        /// <param name="transaction">The transaction to log.</param>
        void LogTransaction(Transaction transaction);

        /// <summary>
        /// Log Exception.
        /// </summary>
        /// <param name="moduleName">The modulename where the exception was thrown.</param>
        /// <param name="typeFullName">The type where the exception was thrown.</param>
        /// <param name="methodName">The method where the exception was thrown.</param>
        /// <param name="lineNumber">The line number where the exception was thrown.</param>
        /// <param name="exceptionType">The exeption type.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        /// <param name="message">the complete exception info.</param>
        void LogException(string moduleName, string typeFullName, string methodName, int lineNumber,
            string exceptionType, string exceptionMessage, string message);
    }
}
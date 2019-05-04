using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AWS.Logger.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AWS.Logger.AspNetCore.Structured.Internals;

namespace AWS.Logger.AspNetCore.Structured
{
    /// <summary>
    /// provides loggers that do augmenting, sutiable for structured logging
    /// </summary>
    public class AugmentingLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly IAugmentingRenderer _augrenderer;
        private readonly IConfiguration _cfg;

        /// <summary>
        /// cons
        /// </summary>
        /// <param name="loggerProvider">the underlying logger provider to use (Generally AWSLoggerProvider)</param>
        /// <param name="renderer">an augmenting renderer, such as StructuredRenderer</param>
        /// <param name="cfg">optional configuration; not used at the present</param>
        public AugmentingLoggerProvider(ILoggerProvider loggerProvider, IAugmentingRenderer renderer, IConfiguration cfg)
        {
            _loggerProvider = loggerProvider;
            _augrenderer = renderer;
            _cfg = cfg;
        }

        /// <summary>
        /// create a logger. it will be of an augmented form.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>logger from the underlying provider, wrapped by an augmenter</returns>
        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            var underlyingLogger = _loggerProvider.CreateLogger(categoryName);
            var uaws = underlyingLogger as AWSLogger;
            if (uaws != null)
            {
                uaws.IncludeScopes = false;
            }

            return new LoggerWrapper(underlyingLogger, _augrenderer, categoryName, _cfg);
        }

        #region IDisposable Support
        /// <summary>
        /// disposal
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _loggerProvider?.Dispose();
            }
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// logger wrapperclass
        /// </summary>
        internal class LoggerWrapper : ILogger
        {
            private readonly ILogger _logger;
            private readonly IAugmentingRenderer _augrenderer;
            private readonly string _category;
            private readonly IConfiguration _cfg;

            /// <summary>
            /// wrap
            /// </summary>
            /// <param name="logger"></param>
            /// <param name="augrenderer"></param>
            /// <param name="category"></param>
            /// <param name="cfg"></param>
            public LoggerWrapper(ILogger logger, IAugmentingRenderer augrenderer, string category, IConfiguration cfg)
            {
                _logger = logger;
                _augrenderer = augrenderer;
                _category = category;
                _cfg = cfg;
            }

            /// <summary>
            /// begin a scope
            /// </summary>
            /// <typeparam name="TState"></typeparam>
            /// <param name="state"></param>
            /// <returns></returns>
            public IDisposable BeginScope<TState>(TState state)
            {
                return LogScope.Push(_category, state);
            }

            /// <summary>
            /// is enabled?
            /// </summary>
            /// <param name="logLevel"></param>
            /// <returns></returns>
            public bool IsEnabled(LogLevel logLevel)
            {
                return _logger.IsEnabled(logLevel);
            }

            /// <summary>
            /// log a msg
            /// </summary>
            /// <typeparam name="TState"></typeparam>
            /// <param name="logLevel"></param>
            /// <param name="eventId"></param>
            /// <param name="_state"></param>
            /// <param name="_exception"></param>
            /// <param name="formatter"></param>
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState _state, Exception _exception, Func<TState, Exception, string> formatter)
            {
                _logger.Log(logLevel, eventId, _state, _exception, (state, exception) =>
                {
                    var prerendered = formatter(state, exception);
                    return _augrenderer.ReRender(logLevel, eventId, prerendered, _state, exception, _category, _cfg);
                });
            }
        }
    }
}

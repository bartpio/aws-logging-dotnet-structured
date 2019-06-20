using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWS.Logger.AspNetCore.Structured.Internals
{
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
        /// instruct augmenter to include scopes?
        /// </summary>
        public bool IncludeScopes { get; set; }

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
            if (_augrenderer.RendersLogScope)
            {
                //here we use LogScope introduced in this library
                return LogScope.Push(_category, state);
            }
            else
            {
                //otherwise passthrough to raw logger BeginScope
                return _logger.BeginScope(state);
            }
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
        /// uses wrapped logger for an initial rendering
        /// then, the re-renderer takes a pass
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="passedState"></param>
        /// <param name="passedException"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState passedState, Exception passedException, Func<TState, Exception, string> formatter)
        {
            const Exception dummyException = null;

            _logger.Log(logLevel, eventId, passedState, dummyException, (arrowState, arrowException) =>
            {
                var prerendered = formatter(arrowState, arrowException);
                
                // the real exception is dealt with by the re-renderer ONLY.
                return _augrenderer.ReRender(logLevel, eventId, prerendered, passedState, IncludeScopes, passedException, _category, _cfg);
            });
        }
    }
}

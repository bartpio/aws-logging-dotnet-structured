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
        /// if set we'll include scopes
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// create a logger. it will be of an augmented form.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>logger from the underlying provider, wrapped by an augmenter</returns>
        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            var underlyingLogger = _loggerProvider.CreateLogger(categoryName);
            return new LoggerWrapper(underlyingLogger, _augrenderer, categoryName, _cfg) { IncludeScopes = IncludeScopes };
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

       
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWS.Logger.AspNetCore.Structured.Tests
{
    public class FakeLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// access to most recently genned logger
        /// </summary>
        public FakeLogger LastLogger { get; set; }

        /// <summary>
        /// cons
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            var result = new FakeLogger();
            LastLogger = result;
            return result;
        }

        public void Dispose()
        {
        }
    }
}

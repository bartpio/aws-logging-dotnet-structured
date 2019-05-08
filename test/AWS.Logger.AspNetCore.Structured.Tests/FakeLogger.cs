using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AWS.Logger.AspNetCore.Structured.Tests
{
    /// <summary>
    /// fake logger for verification of functionality
    /// </summary>
    public class FakeLogger : ILogger
    {
        /// <summary>
        /// not impl
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return new MemoryStream();  //a totally fake scope! for unit purposes only
        }

        /// <summary>
        /// is enabled
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <summary>
        /// just provide access to last logged msg
        /// </summary>
        public string Last { get; set; }

        /// <summary>
        /// "log" by storing to Last property
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var msg = formatter(state, exception);
            Last = msg;
        }
    }
}

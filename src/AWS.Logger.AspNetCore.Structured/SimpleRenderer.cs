using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AWS.Logger.AspNetCore.Structured
{
    /// <summary>
    /// simple renderer augmenter impl
    /// renders to the form:
    /// e.g. WARN [SomeNameSpace.Class] - Something Happened</returns>
    /// </summary>
    public class SimpleRenderer : IAugmentingRenderer
    {
        /// <summary>
        /// here we don't mess with scope directly
        /// </summary>
        bool IAugmentingRenderer.RendersLogScope => false;

        /// <summary>
        /// rerender
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="prerendered"></param>
        /// <param name="state"></param>
        /// <param name="includeScopes">ignored</param>
        /// <param name="exception"></param>
        /// <param name="categoryName"></param>
        /// <param name="cfg"></param>
        /// <returns>
        /// rerendered to the form:
        /// e.g. WARN [SomeNameSpace.Class] - Something Happened
        /// </returns>
        string IAugmentingRenderer.ReRender(LogLevel logLevel, EventId eventId, string prerendered, object state, bool includeScopes, Exception exception, string categoryName, IConfiguration cfg)
        {
            return $"{logLevel.ToString("G").ToUpper()} [{categoryName}] - {prerendered}";
        }
    }
}

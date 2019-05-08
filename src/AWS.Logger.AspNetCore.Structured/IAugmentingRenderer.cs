using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AWS.Logger.AspNetCore.Structured
{
    /// <summary>
    /// Augmenting Renderer interface
    /// </summary>
    public interface IAugmentingRenderer
    {
        /// <summary>
        /// Re-Render a message that has been preliminarily rendered, augmenting or replacing it
        /// </summary>
        /// <param name="logLevel">Log Level</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="prerendered">The previously rendered message. Can be used as a starting point to augment.</param>
        /// <param name="state">Various Logger state information that may be available (this TState)</param>
        /// <param name="includeScopes">if true we're being instructed to render scopes</param>
        /// <param name="exception">Exception, if applicable</param>
        /// <param name="categoryName">Category Name</param>
        /// <param name="cfg">optional configuration; not likely to actually be used</param>
        /// <returns>The newly render message used to replace the original</returns>
        string ReRender(LogLevel logLevel, EventId eventId, string prerendered, object state, bool includeScopes, Exception exception, string categoryName, IConfiguration cfg);

        /// <summary>
        /// true if uses <see cref="AWS.Logger.AspNetCore.Structured.Internals.LogScope" information to render scope />; in this case calls to BeginScope on the underlying logger will be suppressed
        /// </summary>
        bool RendersLogScope { get; }
    }
}

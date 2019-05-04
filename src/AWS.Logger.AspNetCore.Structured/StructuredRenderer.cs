using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Immutable;
using Newtonsoft.Json;
using AWS.Logger.AspNetCore.Structured.Internals;
using System.IO;
using System.Text;

namespace AWS.Logger.AspNetCore.Structured
{
    /// <summary>
    /// structured renderer pumps out JSON formatted msges
    /// </summary>
    public class StructuredRenderer : IAugmentingRenderer
    {
        /// <summary>
        /// our serializer
        /// </summary>
        private readonly JsonSerializer _jss;

        /// <summary>
        /// cons, given a json serializer to hold on to
        /// </summary>
        /// <param name="jss">a JSON serializer to use per message</param>
        public StructuredRenderer(JsonSerializer jss)
        {
            _jss = jss;
        }

        /// <summary>
        /// Re-render a previously rendered raw msg in a JSON form
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="prerendered"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="categoryName"></param>
        /// <param name="cfg"></param>
        /// <returns>JSON-form, with "msg" property bearing the original msg</returns>
        string IAugmentingRenderer.ReRender(LogLevel logLevel, EventId eventId, string prerendered, object state, Exception exception, string categoryName, IConfiguration cfg)
        {
            var scopes = LogScope.Current?.EnumerateScopes() ?? ImmutableList<object>.Empty;
            var (unaries, groupedPairs) = scopes.EvaluateLogstate();
            var obj = new { logLevel, categoryName, msg = prerendered, exception, eventId, tags = unaries, scope = groupedPairs };
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    _jss.Serialize(sw, obj);
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
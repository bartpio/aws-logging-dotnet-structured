using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Linq;
using AWS.Logger.AspNetCore;
using AWS.Logger.AspNetCore.Structured;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AWS.Logger.AspNetCore.Structured.Internals
{
    /// <summary>
    /// Extension Methods to assist with Structured Logging internals
    /// </summary>
    public static class StructuredLoggingExtensions
    {
        /// <summary>
        /// Evaluate logstate for pairs and unaries
        /// </summary>
        /// <param name="state">Logging state; think TState</param>
        /// <returns>unaries (tags) as well as grouped pairs</returns>
        public static (ImmutableList<string> unaries, ImmutableDictionary<string, ImmutableList<Object>> groupedPairs) EvaluateLogstate(this IEnumerable<object> state)
        {
            if (state == null) {
                throw new ArgumentNullException(nameof(state));
            }

            var unaries = ImmutableList<string>.Empty;
            var pairs = ImmutableList<(string Key, object Value)>.Empty;
            foreach (var entry in state.Where(x => x != null))
            {
                if (entry is IEnumerable<KeyValuePair<string, object>> enu)
                {
                    pairs = pairs.AddRange(enu.Select(x => (x.Key, x.Value)));
                }
                else
                {
                    unaries = unaries.Add(entry.ToString());
                }
            }

            var qgrouped = from pair in pairs
                group pair by pair.Key into grp
                select new { grp.Key, Values = grp.Select(x => x.Value)}; 

            return (unaries, qgrouped.ToImmutableDictionary(x => x.Key, x => x.Values.ToImmutableList()));
        }
    }
}
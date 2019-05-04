using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AWS.Logger.AspNetCore.Structured
{
    /// <summary>
    /// Logging Builder Extensions
    /// </summary>
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// json serializer settings
        /// </summary>
        private static JsonSerializerSettings JsonSettings
        {
            get
            {
                var result = new JsonSerializerSettings()
                {
                    Formatting = Formatting.None,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.None
                };

                result.Converters.Add(new StringEnumConverter());
                return result;
            }
        }

        /// <summary>
        /// lazy jss access
        /// </summary>
        private static Lazy<JsonSerializer> _lazyjss = new Lazy<JsonSerializer>(() => JsonSerializer.Create(JsonSettings));

        /// <summary>
        /// Wires up the Logging Builder to AWS Logging
        /// </summary>
        /// <param name="loggingBuilder">dotnet Logging Builder</param>
        /// <param name="awsSection">"AWS" section of the configuration</param>
        /// <returns>The same Logging Builder that was passed in</returns>
        public static ILoggingBuilder AddAWSLogging(this ILoggingBuilder loggingBuilder, IConfigurationSection awsSection)
        {
            if (awsSection != null)
            {
                var awsProvider = new AWSLoggerProvider(awsSection.GetAWSLoggingConfigSection());
                var structuredRenderer = new StructuredRenderer(_lazyjss.Value);
                var augmentedProvider = new AugmentingLoggerProvider(awsProvider, structuredRenderer, awsSection);
                loggingBuilder.AddProvider(augmentedProvider); //We add the AUGMENTED provider.
            }

            //Return the same Logging Builder that was passed in
            return loggingBuilder;
        }
    }
}

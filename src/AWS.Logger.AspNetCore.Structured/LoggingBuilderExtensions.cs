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
        private static readonly Lazy<JsonSerializer> _lazyjss = new Lazy<JsonSerializer>(() => JsonSerializer.Create(JsonSettings));

        /// <summary>
        /// Wires up the Logging Builder to AWS Logging, specifically using the StructuredRenderer
        /// </summary>
        /// <param name="loggingBuilder">dotnet Logging Builder</param>
        /// <param name="awsSection">"AWS" section of the configuration</param>
        /// <returns>The same Logging Builder that was passed in</returns>
        public static ILoggingBuilder AddAwsLoggingStructured(this ILoggingBuilder loggingBuilder, IConfigurationSection awsSection)
        {
            if (awsSection != null)
            {
                var structuredRenderer = new StructuredRenderer(_lazyjss.Value);
                (var awsConfig, var includeScopes) = awsSection.GetAWSLoggingConfigSection().ProcessIncludeScopes(structuredRenderer);
                var awsProvider = new AWSLoggerProvider(awsConfig);
                var augmentedProvider = new AugmentingLoggerProvider(awsProvider, structuredRenderer, awsSection) { IncludeScopes = includeScopes };
                loggingBuilder.AddProvider(augmentedProvider); //We add the AUGMENTED provider.
            }

            //Return the same Logging Builder that was passed in
            return loggingBuilder;
        }

        /// <summary>
        /// add aws logging given instance of any IAugmentingRenderer
        /// </summary>
        /// <typeparam name="TAugmenter"></typeparam>
        /// <param name="loggingBuilder"></param>
        /// <param name="awsSection"></param>
        /// <param name="augmentedRenderer">the IAugmentingRenderer instance to use</param>
        /// <returns>same ILoggingBuilder that was passed</returns>
        public static ILoggingBuilder AddAwsLogging<TAugmenter>(this ILoggingBuilder loggingBuilder, IConfigurationSection awsSection, TAugmenter augmentedRenderer) where TAugmenter :  IAugmentingRenderer
        {
            if (awsSection != null)
            {
                (var awsConfig, var includeScopes) = awsSection.GetAWSLoggingConfigSection().ProcessIncludeScopes(augmentedRenderer);
                var awsProvider = new AWSLoggerProvider(awsConfig);
                var augmentedProvider = new AugmentingLoggerProvider(awsProvider, augmentedRenderer, awsSection) { IncludeScopes = includeScopes };
                loggingBuilder.AddProvider(augmentedProvider); //We add the AUGMENTED provider.
            }

            //Return the same Logging Builder that was passed in
            return loggingBuilder;
        }

        /// <summary>
        /// Add Aws Logging - simplistic (although less basic than default AWS impl which logs msg ONLY)
        /// renders to the form:
        /// e.g. WARN [SomeNameSpace.Class] - Something Happened</returns>
        /// </summary>
        /// <param name="loggingBuilder"></param>
        /// <param name="awsSection"></param>
        /// <returns>same ILoggingBuilder that was passed</returns>
        public static ILoggingBuilder AddAwsLoggingSimple(this ILoggingBuilder loggingBuilder, IConfigurationSection awsSection)
        {
            var augmentedRenderer = new SimpleRenderer();
            return AddAwsLogging(loggingBuilder, awsSection, augmentedRenderer);
        }

        /// <summary>
        /// if the augmenter is known to process LogScope using our own way, disable IncludeScopes for the underlying loggerprovider no matter what
        /// </summary>
        /// <param name="awsConfig"></param>
        /// <param name="augmenter"></param>
        /// <returns>the same awsConfig passed in; possibly mutated! also, original value of IncludeScopes</returns>
        internal static (AWSLoggerConfigSection, bool) ProcessIncludeScopes(this AWSLoggerConfigSection awsConfig, IAugmentingRenderer augmenter)
        {
            var originalIncludeScopes = awsConfig.IncludeScopes;

            if (augmenter.RendersLogScope)
            {
                awsConfig.IncludeScopes = false;
            }

            return (awsConfig, originalIncludeScopes);
        }
    }
}

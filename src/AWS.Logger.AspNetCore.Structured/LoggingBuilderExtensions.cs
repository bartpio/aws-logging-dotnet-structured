using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.CompilerServices;
using AWS.Logger.AspNetCore.Structured.Converters;

//provide Tests library with access to internals (we happen do to this here; it applies at the entire assembly scope)
[assembly: InternalsVisibleTo("AWS.Logger.AspNetCore.Structured.Tests")]

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
        internal static JsonSerializerSettings JsonSettings
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
                result.Converters.Add(new ExceptionStringConverter());
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
        /// <param name="configSection">configuration root in which we are looking for the block specified by configSectionInfoBlockName</param>
        /// <param name="configSectionInfoBlockName">config section info block name, ex. AWS.Logging or AWS</param>
        /// <param name="awsLoggerConfigAction">optional aws logger reconfigurator action</param>
        /// <returns>The same Logging Builder that was passed in</returns>
        public static ILoggingBuilder AddAwsLoggingStructured(this ILoggingBuilder loggingBuilder, IConfiguration configSection, string configSectionInfoBlockName, Action<AWSLoggerConfig> awsLoggerConfigAction = null)
        {
            if (loggingBuilder == null)
            {
                throw new ArgumentNullException(nameof(loggingBuilder));
            }
            if (configSection == null)
            {
                throw new ArgumentNullException(nameof(configSection));
            }

            var structuredRenderer = new StructuredRenderer(_lazyjss.Value);
            (var awsConfig, var includeScopes) = configSection.GetAWSLoggingConfigSection(configSectionInfoBlockName).ProcessIncludeScopes(structuredRenderer);
            if (awsConfig != null)
            {
                awsLoggerConfigAction?.Invoke(awsConfig.Config);
                var awsProvider = new AWSLoggerProvider(awsConfig);
                var augmentedProvider = new AugmentingLoggerProvider(awsProvider, structuredRenderer, configSection) { IncludeScopes = includeScopes };
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
        /// <param name="configSectionInfoBlockName">config section info block name, ex. AWS.Logging or AWS</param>
        /// <param name="configSection">configuration root in which we are looking for the block specified by configSectionInfoBlockName</param>
        /// <param name="augmentedRenderer">the IAugmentingRenderer instance to use</param>
        /// <param name="awsLoggerConfigAction">optional aws logger reconfigurator action</param>
        /// <returns>same ILoggingBuilder that was passed</returns>
        public static ILoggingBuilder AddAwsLogging<TAugmenter>(this ILoggingBuilder loggingBuilder, IConfiguration configSection, string configSectionInfoBlockName, TAugmenter augmentedRenderer, Action<AWSLoggerConfig> awsLoggerConfigAction = null) where TAugmenter : IAugmentingRenderer
        {
            if (loggingBuilder == null)
            {
                throw new ArgumentNullException(nameof(loggingBuilder));
            }
            if (configSection == null)
            {
                throw new ArgumentNullException(nameof(configSection));
            }

            (var awsConfig, var includeScopes) = configSection.GetAWSLoggingConfigSection(configSectionInfoBlockName).ProcessIncludeScopes(augmentedRenderer);
            if (awsConfig != null)
            {
                awsLoggerConfigAction?.Invoke(awsConfig.Config);
                var awsProvider = new AWSLoggerProvider(awsConfig);
                var augmentedProvider = new AugmentingLoggerProvider(awsProvider, augmentedRenderer, configSection) { IncludeScopes = includeScopes };
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
        /// <param name="configSectionInfoBlockName">config section info block name, ex. AWS.Logging or AWS</param>
        /// <param name="configSection">configuration root in which we are looking for the block specified by configSectionInfoBlockName</param>
        /// <param name="awsLoggerConfigAction">optional aws logger reconfigurator action</param>
        /// <returns>same ILoggingBuilder that was passed</returns>
        public static ILoggingBuilder AddAwsLoggingSimple(this ILoggingBuilder loggingBuilder, string configSectionInfoBlockName, IConfiguration configSection, Action<AWSLoggerConfig> awsLoggerConfigAction = null)
        {
            var augmentedRenderer = new SimpleRenderer();
            return AddAwsLogging(loggingBuilder, configSection, configSectionInfoBlockName, augmentedRenderer, awsLoggerConfigAction);
        }

        /// <summary>
        /// if the augmenter is known to process LogScope using our own way, disable IncludeScopes for the underlying loggerprovider no matter what
        /// </summary>
        /// <param name="awsConfig"></param>
        /// <param name="augmenter"></param>
        /// <returns>the same awsConfig passed in; possibly mutated! also, original value of IncludeScopes</returns>
        internal static (AWSLoggerConfigSection, bool) ProcessIncludeScopes(this AWSLoggerConfigSection awsConfig, IAugmentingRenderer augmenter)
        {
            if (awsConfig != null)
            {
                var originalIncludeScopes = awsConfig.IncludeScopes;

                if (augmenter.RendersLogScope)
                {
                    awsConfig.IncludeScopes = false;
                }

                return (awsConfig, originalIncludeScopes);
            }
            else
            {
                return (null, false);
            }
        }
    }
}

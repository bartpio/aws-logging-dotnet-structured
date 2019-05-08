# aws-logging-dotnet-structured
enhances .NET Libraries for integrating Amazon CloudWatch Logs with Structured Logging support, using JSON.

This library provides structured Logging support suitable for AWS Logging from within AspNetCore. Structured Logs are in JSON form, including a rendered message, unary scope properties (tags), and KeyValuePair scope properties. An extention method is provided to register a structed AWS log provider; other than that there's no real AWS dependency here. The AWS Logging configuration provided should NOT include a custom formatter.

Use AddAwsLoggingStructured extension method (namespace AWS.Logger.AspNetCore.Structured) to add structured AWS logging to an ILoggingBuilder.
Use AddAwsLoggingSimple extension method to add AWS logging that isn't structured (and isn't JSON), but is augmented with the log level and category name, in the form LEVEL [Category] - Rest of message
Optionally, implement IAugmentingRenderer, and pass it to an AugmentingLoggerProvider instance, in order to provide structured or unstructured (augmented with additional facts as desired) AWS logging. 

AWS CloudWatch Insight query service works great for querying the resulting structured logs, including discovered property support.
Example filter: 
scope.RequestID like /123/
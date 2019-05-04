# aws-logging-dotnet-structured
enhances .NET Libraries for integrating Amazon CloudWatch Logs with Structured Logging support, using JSON.

This library provides structured Logging support suitable for AWS Logging from within AspNetCore. Logs in JSON form, including a rendered message, unary scope properties (tags), and KeyValuePair scope properties. An extention method is provided to register a structed AWS log provider; other than that there's no real AWS dependency here. The AWS Logging configuration provided should NOT include a custom formatter.

AWS CloudWatch Insight query service works great for querying the resulting lots, including discovered property support.
Example filter: 
scope.RequestID like /123/
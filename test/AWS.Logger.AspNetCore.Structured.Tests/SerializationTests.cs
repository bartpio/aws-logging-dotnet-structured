using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;

namespace AWS.Logger.AspNetCore.Structured.Tests
{
    /// <summary>
    /// serialization (as it's used for structued logging) tests
    /// </summary>
    [TestFixture]
    public class SerializationTests
    {
        /// <summary>
        /// check that ExceptionStringConverter is installed in the JsonSettings that we use for structured logging,
        /// at the same time checking its intended side effect, which is to prevent massive detailed output when serialzing
        /// certain Exception instances the default way
        /// </summary>
        [Test]
        public void CheckExceptionStringConverter()
        {
            try
            {
                throw new InvalidOperationException("test inv");
            }
            catch (InvalidOperationException testException)
            {
                var jsonSettings = LoggingBuilderExtensions.JsonSettings;
                var jss = JsonSerializer.Create(jsonSettings);
                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms))
                    {
                        jss.Serialize(sw, testException);
                    }

                    var str = Encoding.UTF8.GetString(ms.ToArray());
                    StringAssert.Contains("test inv", str);
                    StringAssert.DoesNotContain("\"Message\"", str);
                }
            }
        }
    }
}

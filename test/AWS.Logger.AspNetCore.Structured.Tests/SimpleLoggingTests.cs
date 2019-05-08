
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
using System.Collections.Generic;

namespace AWS.Logger.AspNetCore.Structured.Tests
{
    [TestFixture]
    public class SimpleLoggingTests
    {
        [Test]
        public void TestUsingConsole()
        {
            var sr = new SimpleRenderer();
            var con = new ConsoleLoggerProvider((x, y) => true, false, true);
            ILoggerProvider alp = new AugmentingLoggerProvider(con, sr, null) { IncludeScopes = true };
            var augged = alp.CreateLogger("tests");
            augged.LogInformation("Hello world.");
            using (augged.BeginScope("tagscope"))
            {
                augged.LogInformation("Hello world from within.");
            }

            using (augged.BeginScope(new Dictionary<string, object>()
            {
                ["CustomerId"] = 4444,
                ["OrderId"] = 777
            }))
            {
                augged.LogInformation("Searching for order status");
            }
        }

        /// <summary>
        /// test using fakes
        /// </summary>
        [Test]
        public void TestUsingFakes()
        {
            var sr = new SimpleRenderer();
            var fakeprov = new FakeLoggerProvider();
            ILoggerProvider alp = new AugmentingLoggerProvider(fakeprov, sr, null) { IncludeScopes = true };
            var augged = alp.CreateLogger("tests");
            augged.LogInformation("Hello world.");
            StringAssert.Contains("Hello", fakeprov.LastLogger.Last);

            augged.LogInformation("Hello world from within.");
            StringAssert.Contains("Hello", fakeprov.LastLogger.Last);
            StringAssert.Contains("[tests]", fakeprov.LastLogger.Last);  //test SimpleRenderer category render
        }
    }
}

using AWS.Logger.AspNetCore.Structured;
using AWS.Logger.AspNetCore.Structured.Tests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestUsingConsole()
        {
            var jss = new JsonSerializer();
            var sr = new StructuredRenderer(jss);
            var con = new ConsoleLoggerProvider((x, y) => true, false, true);
            ILoggerProvider alp = new AugmentingLoggerProvider(con, sr, null);
            var augged = alp.CreateLogger("tests");
            augged.LogInformation("Hello world.");
            using (augged.BeginScope("tagscope"))
            {
                augged.LogInformation("Hello world from within.");
            }

            using (augged.BeginScope(new Dictionary<string, object>() {
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
            var jss = new JsonSerializer();
            var sr = new StructuredRenderer(jss);
            var fakeprov = new FakeLoggerProvider();
            ILoggerProvider alp = new AugmentingLoggerProvider(fakeprov, sr, null);
            var augged = alp.CreateLogger("tests");
            augged.LogInformation("Hello world.");
            StringAssert.Contains("Hello", fakeprov.LastLogger.Last);

            using (augged.BeginScope("tagscope"))
            {
                augged.LogInformation("Hello world from within.");
                StringAssert.Contains("Hello", fakeprov.LastLogger.Last);
                StringAssert.Contains("tagscope", fakeprov.LastLogger.Last);
            }

            using (augged.BeginScope(new Dictionary<string, object>() {
                ["CustomerId"] = 4444,
                ["OrderId"] = 777
            }))
            {
                augged.LogInformation("Searching for order status");
                StringAssert.Contains("status", fakeprov.LastLogger.Last);
                StringAssert.Contains("4444", fakeprov.LastLogger.Last);
                StringAssert.Contains("777", fakeprov.LastLogger.Last);
            }
        }
    }
}
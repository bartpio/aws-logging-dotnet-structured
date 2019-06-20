using AWS.Logger.AspNetCore.Structured.Converters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace AWS.Logger.AspNetCore.Structured.Tests
{
    [TestFixture]
    public class JsonTests
    {
        [Test]
        public void TestExceptionConverter()
        {
            var conv = new ExceptionStringConverter();
            var jss = new JsonSerializerSettings() { Formatting = Formatting.None };
            jss.Converters.Add(conv);

            try
            {
                throw new OverflowException("artificial overflow situation");
            }
            catch (OverflowException exc)
            {
                var obj = new { exc };

                var json = JsonConvert.SerializeObject(obj, jss);
                StringAssert.DoesNotContain("\n", json);
                StringAssert.DoesNotContain("\r", json);
                return;
            }

            //can't happen.
            Assert.Fail("Expected an exception above");
        }
    }
}

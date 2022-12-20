using MCRA.Utils.DataSourceReading.ValueConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Utils.Tests.UnitTests.DataReading.ValueConversion {
    [TestClass]
    public class DateTimeValueConverterTests {

        [TestMethod]
        [DataRow("1-2-2001", 2001, 2, 1)]
        [DataRow("31-12-2001", 2001, 12, 31)]
        [DataRow("1/2/2001", 2001, 2, 1)]
        [DataRow("31/12/2001", 2001, 12, 31)]
        [DataRow("1-2-2001 11:30", 2001, 2, 1)]
        [DataRow("01-02-2001 11:30", 2001, 2, 1)]
        [DataRow("1/2/2001 13:30", 2001, 2, 1)]
        [DataRow("1-2-2001 15:00:00", 2001, 2, 1)]
        [DataRow("1/2/2001 15:00:00", 2001, 2, 1)]
        [DataRow("1-2-2001 0:00", 2001, 2, 1)]
        [DataRow("1/2/2001 15:00", 2001, 2, 1)]
        [DataRow("1-2-2001", 2001, 2, 1)]
        [DataRow("2001-2-1", 2001, 2, 1)]
        [DataRow("2001-02-01", 2001, 2, 1)]
        [DataRow("2007-12-31 15:00:00", 2007, 12, 31)]
        [DataRow("2008-03-09T16:05:07", 2008, 3, 9)]
        [DataRow("2008-03-09 16:05:07Z", 2008, 3, 9)]
        [DataRow("3-10-2020", 2020, 10, 3)]
        [DataRow("2020-3-10", 2020, 3, 10)]
        [DataRow("2020-3-10", 2020, 3, 10)]
        [DataRow("08/12/2018 07:22:16", 2018, 12, 8)]
        [DataRow("2018-08-18T07:22:16.0000000", 2018, 8, 18)]
        [DataRow("2018-08-18T07:22:16.0000000Z", 2018, 8, 18)]
        [DataRow("2018-08-18T07:22:16.0000000-07:00", 2018, 8, 18)]
        [DataRow("Sat, 18 Aug 2018 07:22:16 GMT", 2018, 8, 18)]
        public void DateTimeValueConverter_TestConvert(string str, int year, int month, int day) {
            var converter = new DateTimeValueConverter();
            Assert.AreEqual(
                (new DateTime(year, month, day)).ToShortDateString(),
                ((DateTime)converter.Convert(str)).ToShortDateString()
            );
        }

        [TestMethod]
        public void DateTimeValueConverter_TestReturnType() {
            var converter = new DateTimeValueConverter();
            Assert.AreEqual(typeof(DateTime), converter.Convert("10-3-2020").GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        [DataRow("invalid")]
        [DataRow("123x")]
        [DataRow("f")]
        [DataRow("2007-31-12")]
        [DataRow("2007/31/12")]
        [DataRow("1/31/12")]
        [DataRow("1-31-12")]
        public void DateTimeValueConverter_TestFail(string str) {
            var converter = new DateTimeValueConverter();
            converter.Convert(str);
        }
    }
}

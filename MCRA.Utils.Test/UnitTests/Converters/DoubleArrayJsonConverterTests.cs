using MCRA.Utils.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biometris.Test.UnitTests.Converters {
    [TestClass]
    public class DoubleArrayJsonConverterTests {
        public class TestValues {
            [JsonConverter(typeof(DoubleArrayJsonConverter))]
            public double[] ValueArray { get; set; } = Array.Empty<double>();
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArrayNullValueTest() {
            const string json = "{\"ValueArray\": null }";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.IsNull(testValue.ValueArray);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArrayNullArrayTest() {
            const string json = "{\"ValueArray\": [ null ]}";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue?.ValueArray);
            Assert.IsEmpty(testValue.ValueArray);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArrayNanValueTest() {
            const string json = "{\"ValueArray\": \"NA\" }";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(1, testValue.ValueArray);
            Assert.AreEqual(double.NaN, testValue.ValueArray[0]);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArrayNanArrayTest() {
            const string json = "{\"ValueArray\": [ \"NA\" ]}";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(1, testValue.ValueArray);
            Assert.AreEqual(double.NaN, testValue.ValueArray[0]);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArraySingleValueTest() {
            const string json = "{\"ValueArray\": 3.23930E-89}";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(1, testValue.ValueArray);
            Assert.AreEqual(3.23930E-89, testValue.ValueArray[0]);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArrayValueArrayTest() {
            const string json = "{\"ValueArray\": [ 3.390123, -903.4239, \"NA\", 43.24E-222 ]}";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(4, testValue.ValueArray);
            Assert.AreEqual(3.390123, testValue.ValueArray[0]);
            Assert.AreEqual(-903.4239, testValue.ValueArray[1]);
            Assert.AreEqual(double.NaN, testValue.ValueArray[2]);
            Assert.AreEqual(43.24E-222, testValue.ValueArray[3]);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArraySingleValueMatrixTest() {
            const string json = "{\"ValueArray\": [ [ 3.23930E-89  ] ]}";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(1, testValue.ValueArray);
            Assert.AreEqual(3.23930E-89, testValue.ValueArray[0]);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArrayValueMatrixTest() {
            const string json = "{\"ValueArray\": [ [ 3.390123 ], -903.4239, [ \"NA\", 43.24E-222 ] ]}";

            var converter = new DoubleArrayJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(4, testValue.ValueArray);
            Assert.AreEqual(3.390123, testValue.ValueArray[0]);
            Assert.AreEqual(-903.4239, testValue.ValueArray[1]);
            Assert.AreEqual(double.NaN, testValue.ValueArray[2]);
            Assert.AreEqual(43.24E-222, testValue.ValueArray[3]);
        }

        [TestMethod]
        public void DoubleArrayJsonConverter_ArrayDoubleLimitsArrayTest() {
            var minValueString = double.MinValue.ToString(CultureInfo.InvariantCulture);
            var maxValueString = double.MaxValue.ToString(CultureInfo.InvariantCulture);
            string json = $"{{\"ValueArray\": [ \"NA\", \"Inf\", \"-Inf\", 0, {minValueString}, {maxValueString}]}}";

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DoubleArrayJsonConverter());

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(6, testValue.ValueArray);
            Assert.AreEqual(double.NaN, testValue.ValueArray[0]);
            Assert.AreEqual(double.PositiveInfinity, testValue.ValueArray[1]);
            Assert.AreEqual(double.NegativeInfinity, testValue.ValueArray[2]);
            Assert.AreEqual(0D, testValue.ValueArray[3]);
            Assert.AreEqual(double.MinValue, testValue.ValueArray[4]);
            Assert.AreEqual(double.MaxValue, testValue.ValueArray[5]);
        }
    }
}

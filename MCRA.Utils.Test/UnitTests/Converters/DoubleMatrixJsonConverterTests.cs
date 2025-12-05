using MCRA.Utils.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biometris.Test.UnitTests.Converters {
    [TestClass]
    public class DoubleMatrixJsonConverterTests {
        public class TestValues {
            [JsonConverter(typeof(DoubleMatrixJsonConverter))]
            public double[][] ValueMatrix { get; set; } = Array.Empty<double[]>();
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_NoValueTest() {
            const string json = "{ }";

            var converter = new DoubleMatrixJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.IsNotNull(testValue.ValueMatrix);
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_ArrayNullValueTest() {
            const string json = "{\"ValueMatrix\": null }";

            var converter = new DoubleMatrixJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.IsNull(testValue.ValueMatrix);
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_ArrayNullTest() {
            const string json = "{\"ValueMatrix\": [ null ]}";

            var converter = new DoubleMatrixJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue?.ValueMatrix);
            Assert.IsEmpty(testValue.ValueMatrix);
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_ArrayNanValueTest() {
            const string json = "{\"ValueMatrix\": [ [ \"NA\" ] ] }";

            var converter = new DoubleMatrixJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(1, testValue.ValueMatrix);
            Assert.HasCount(1, testValue.ValueMatrix[0]);
            Assert.AreEqual(double.NaN, testValue.ValueMatrix[0][0]);
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_ArrayNullArrayTest() {
            const string json = "{\"ValueMatrix\": [ [ null ] ] }";

            var converter = new DoubleMatrixJsonConverter();

            var options = new JsonSerializerOptions();
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(1, testValue.ValueMatrix);
            Assert.HasCount(1, testValue.ValueMatrix[0]);
            Assert.AreEqual(double.NaN, testValue.ValueMatrix[0][0]);
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_Array2xNullArrayTest() {
            const string json = "{\"ValueMatrix\": [ [ null, null ] ], \"CS\": [\"sfd\"] }";

            var converter = new DoubleMatrixJsonConverter();

            var options = new JsonSerializerOptions {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(converter);

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(1, testValue.ValueMatrix);
            Assert.HasCount(2, testValue.ValueMatrix[0]);
            Assert.AreEqual(double.NaN, testValue.ValueMatrix[0][0]);
            Assert.AreEqual(double.NaN, testValue.ValueMatrix[0][1]);
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_ArrayDoubleLimitsArrayTest() {
            var minValueString = double.MinValue.ToString(CultureInfo.InvariantCulture);
            var maxValueString = double.MaxValue.ToString(CultureInfo.InvariantCulture);
            string json = $"{{\"ValueMatrix\": [ [ \"NA\", \"Inf\", \"-Inf\" ] , [ 0, {minValueString}, {maxValueString}] ]}}";

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DoubleMatrixJsonConverter());

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(2, testValue.ValueMatrix);
            Assert.HasCount(3, testValue.ValueMatrix[0]);
            Assert.HasCount(3, testValue.ValueMatrix[1]);

            Assert.AreEqual(double.NaN, testValue.ValueMatrix[0][0]);
            Assert.AreEqual(double.PositiveInfinity, testValue.ValueMatrix[0][1]);
            Assert.AreEqual(double.NegativeInfinity, testValue.ValueMatrix[0][2]);
            Assert.AreEqual(0D, testValue.ValueMatrix[1][0]);
            Assert.AreEqual(double.MinValue, testValue.ValueMatrix[1][1]);
            Assert.AreEqual(double.MaxValue, testValue.ValueMatrix[1][2]);
        }

        [TestMethod]
        public void DoubleMatrixJsonConverter_ArrayEmptyArraysTest() {
            string json = $"{{\"ValueMatrix\": [ [ ] , [ ] ]}}";

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DoubleMatrixJsonConverter());

            var testValue = JsonSerializer.Deserialize<TestValues>(json, options);

            Assert.IsNotNull(testValue);
            Assert.HasCount(2, testValue.ValueMatrix);
            Assert.IsEmpty(testValue.ValueMatrix[0]);
            Assert.IsEmpty(testValue.ValueMatrix[1]);
        }
    }
}

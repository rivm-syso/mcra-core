using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Compiled.Test {
    [TestClass]
    public class DoseResponseExperimentsConverterTests {

        [TestMethod]
        public void DoseResponseExperimentToDataTableTest() {
            var C1 = new Compound() {
                Code = "C1",
                Name = "C1"
            };
            var C2 = new Compound() {
                Code = "C2",
                Name = "C2"
            };
            var dose1 = new Dictionary<Compound, double> {
                { C2, 2 }
            };

            var dose2 = new Dictionary<Compound, double> {
                { C1, 1 }
            };

            var response1 = new Response() { Code = "R1" };
            var response2 = new Response() { Code = "R2" };
            var measurementsResponse1 = new Dictionary<Response, DoseResponseExperimentMeasurement>();
            var measurementsResponse2 = new Dictionary<Response, DoseResponseExperimentMeasurement>();
            var measurement = new DoseResponseExperimentMeasurement() {
                ResponseValue = 100,
                ResponseSD = 2,
                ResponseN = 10,
            };
            measurementsResponse1.Add(response1, measurement);
            measurementsResponse2.Add(response2, measurement);

            var experimentalUnits = new List<ExperimentalUnit>();
            var expUnit1 = new ExperimentalUnit() {
                Code = "u1",
                Doses = dose1,
                Responses = measurementsResponse1,
            };
            var expUnit2 = new ExperimentalUnit() {
                Code = "u2",
                Doses = dose2,
                Responses = measurementsResponse2,
            };

            experimentalUnits.Add(expUnit1);
            experimentalUnits.Add(expUnit2);

            var doseResponseExperiments = new DoseResponseExperiment() {
                ExperimentalUnits = experimentalUnits,
                Substances = [C1, C2],
            };
            var isMixture = doseResponseExperiments.ExperimentalUnits.Select(c => c.Doses.Count(d => d.Value > 0)).Any(r => r > 1);
            var dataTable = doseResponseExperiments.toDataTable(response1, isMixture);

            Assert.IsNotNull(dataTable);

            var outputPath = TestResourceUtilities.ConcatWithOutputPath("DoseResponseData");
            if (!Directory.Exists(outputPath)) {
                Directory.CreateDirectory(outputPath);
                Thread.Sleep(10);
            }
            var filename = Path.Combine(outputPath, "DoseResponse.csv");
            dataTable.ToCsv(filename);
            Assert.IsTrue(File.Exists(filename));
        }
    }
}

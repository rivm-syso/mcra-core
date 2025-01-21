using System.IO.Compression;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General;
using MCRA.General.DoseResponseModels;
using MCRA.Simulation.Calculators.DoseResponseModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DoseResponseModelCalculation {
    /// <summary>
    /// DoseResponseModelCalculation calculator
    /// </summary>
    [TestClass]
    public class ProastDoseResponseModelCalculationTests {
        /// <summary>
        /// Instantiates Proast dose response model calculator
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestProastAvailable() {
            var calculator = new ProastDoseResponseModelCalculator();
            Assert.IsTrue(calculator.CheckProastAvailable());
        }
        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestAdipo72h
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestAdipo72h() {
            var selectedExperimentCodes = new[] { "Adipo72h" };
            var idTest = "ProastDoseResponseModelCalculation_TestAdipo72h";
            testFitModel(idTest, selectedExperimentCodes);
        }

        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestERODExp
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestERODExp() {
            var selectedExperimentCodes = new[] { "ERODExp" };
            var idTest = "ProastDoseResponseModelCalculation_TestERODExp";
            testFitModel(idTest, selectedExperimentCodes);
        }
        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestfoetalExp
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestfoetalExp() {
            var selectedExperimentCodes = new[] { "foetalExp" };
            var idTest = "ProastDoseResponseModelCalculation_TestfoetalExp";
            testFitModel(idTest, selectedExperimentCodes);
        }
        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestfoetalExp2
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestfoetalExp2() {
            var selectedExperimentCodes = new[] { "foetalExp2" };
            var idTest = "ProastDoseResponseModelCalculation_TestfoetalExp2";
            testFitModel(idTest, selectedExperimentCodes);
        }
        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestidQuantal
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestidQuantal() {
            var selectedExperimentCodes = new[] { "idQuantal" };
            var idTest = "ProastDoseResponseModelCalculation_TestidQuantal";
            testFitModel(idTest, selectedExperimentCodes);
        }
        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestidQuantalWithCovariates
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestidQuantalWithCovariates() {
            var selectedExperimentCodes = new[] { "idQuantal" };
            var idTest = "ProastDoseResponseModelCalculation_TestidQuantalWithCovariates";
            testFitModel(idTest, selectedExperimentCodes, true);
        }
        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestMixture
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestMixture() {
            var selectedExperimentCodes = new[] { "Mixture" };
            var idTest = "ProastDoseResponseModelCalculation_TestMixture";
            testFitModel(idTest, selectedExperimentCodes);
        }
        /// <summary>
        /// Fit model: ProastDoseResponseModelCalculation_TestMixture2
        /// </summary>
        [TestMethod]
        public void ProastDoseResponseModelCalculation_TestidMixture2() {
            var selectedExperimentCodes = new[] { "Mixture2" };
            var idTest = "ProastDoseResponseModelCalculation_TestMixture2";
            testFitModel(idTest, selectedExperimentCodes);
        }


        private static RawDoseResponseModelData testFitModel(string idTest, string[] selectedExperimentCodes, bool fitCovariates = false) {
            var outputPath = TestUtilities.CreateTestOutputPath(idTest);
            var rawDataFolder = Path.Combine("Resources", "DoseResponseData");
            var rawDataZip = Path.Combine(outputPath, "RawDoseResponseData.Zip");
            ZipFile.CreateFromDirectory(rawDataFolder, rawDataZip, CompressionLevel.Optimal, false);
            var dataFolder = Path.Combine(outputPath, "DoseResponseData");
            TestUtilities.CopyRawDataTablesToFolder(rawDataZip, dataFolder);
            var targetFileName = Path.Combine(outputPath, "DoseResponseData.zip");
            var dataManager = TestUtilities.CompiledDataManagerFromFolder(dataFolder, targetFileName);
            var experiments = dataManager.GetAllDoseResponseExperiments();
            var models = new List<DoseResponseModel>();
            foreach (var experiment in experiments.Values.Where(r => selectedExperimentCodes?.Contains(r.Code) ?? true)) {
                var referenceCompound = experiment.Substances.First();
                foreach (var response in experiment.Responses) {
                    var proastDrmCalculator = new ProastDoseResponseModelCalculator(outputPath);
                    var bmrType = (response.ResponseType == ResponseType.Quantal) ? BenchmarkResponseType.ExtraRisk : BenchmarkResponseType.Factor;
                    var bmr = (response.ResponseType == ResponseType.Quantal) ? 0.05 : 0.95;
                    var result = proastDrmCalculator.TryCompute(experiment, response, bmr, bmrType, experiment.Covariates, referenceCompound, 5, fitCovariates);
                    models.AddRange(result);
                    Assert.IsTrue(result.All(r => string.IsNullOrEmpty(r.ExceptionMessage)));
                    Assert.IsTrue(result.All(r => !string.IsNullOrEmpty(r.ModelEquation)));

                    foreach (var drm in result) {
                        foreach (var bmd in drm.DoseResponseModelBenchmarkDoses.Values) {
                            Assert.IsFalse(double.IsNaN(bmd.BenchmarkDose));
                            Assert.IsFalse(double.IsNaN(bmd.BenchmarkResponse));
                            Assert.IsFalse(string.IsNullOrEmpty(bmd.ModelParameterValues));
                        }
                    }

                    foreach (var model in models) {
                        var section = new DoseResponseModelSection();
                        section.Summarize(model, experiment, response, referenceCompound, null);

                        var chart = new DoseResponseFitChartCreator(section, 500, 500, true);
                        var modelName = model.Name.Replace(":", string.Empty);
                        chart.CreateToPng(Path.Combine(outputPath, $"{experiment.Code}-{response.Code}-{modelName}-fit-mcra.png"));
                    }
                }
            }

            var mapper = new RawDoseResponseModelDataConverter();
            var data = mapper.ToRaw(models);
            Assert.IsNotNull(data);
            //CsvRawDataWriter.ExportZippedCsv(data, Path.Combine(outputPath, "DoseResponseModelsArtificial.zip"));
            return data;
        }
        /// <summary>
        /// Calculate parameter b from a, c and d using exponential
        /// </summary>
        [TestMethod]
        public void DoseResponseModelFunction_TestComputeB() {
            var a = 1.0242;
            var b = 0.7277;
            var c = 8.6982;
            var d = 1.2315;
            var sigma = 0.25;
            var s = 1;

            var originalParameters = new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "c", c },
                { "d", d },
                { "sigma", sigma },
                { "s", s },
            };

            var newParameters = new Dictionary<string, double>() {
                { "a", a },
                { "c", c },
                { "d", d },
                { "sigma", sigma },
                { "s", s },
            };

            var bmd = 1.8796;

            var enumValues = Enum.GetValues(typeof(DoseResponseModelType))
                .Cast<DoseResponseModelType>()
                .Where(r => r != DoseResponseModelType.Unknown)
                .ToList();
            foreach (var modelType in enumValues) {
                if (modelType != DoseResponseModelType.TwoStage &&
                    modelType != DoseResponseModelType.Weibull &&
                    modelType != DoseResponseModelType.LogProb &&
                    modelType != DoseResponseModelType.Gamma) {
                    var originalModel = DoseResponseModelFactory.Create(modelType, originalParameters);

                    var bmr = originalModel.Calculate(bmd);
                    var newModel = DoseResponseModelFactory.Create(modelType, newParameters, bmd, bmr);
                    var newModelParameters = newModel.ExportParameters();
                    if (newModelParameters.ContainsKey("b")) {
                        Assert.AreEqual(newModelParameters["b"], b, 1e-5);
                    }
                }
            }
        }

        /// <summary>
        /// Calculate parameter b from a, c and d using TwoStage
        /// </summary>
        [TestMethod]
        public void DoseResponseModelFunction_TestComputeB_TwoStage() {
            testCreateTwoStage(a: 0.0115, b: 8.03, c: 80.5, s: 1, bmd: 0.691);
            testCreateTwoStage(a: .0569, b: 59.59, c: 0, s: 1, bmd: 6.27);
            testCreateTwoStage(a: .0569, b: 59.59, c: 0.1, s: 1, bmd: 6.27);
        }

        private static void testCreateTwoStage(double a, double b, double c, double s, double bmd) {
            var modelType = DoseResponseModelType.TwoStage;
            var originalParameters = new Dictionary<string, double>() {
                { "a", a },
                { "b", b },
                { "c", c },
                { "s", s }
            };

            // Create original model and compute bmr
            var originalModel = DoseResponseModelFactory.Create(modelType, originalParameters);
            var bmr = originalModel.Calculate(bmd);

            // Test derive b
            var newParameters = new Dictionary<string, double>() {
                { "a", originalParameters["a"] },
                { "c", originalParameters["c"] },
                { "s", originalParameters["s"] },
            };
            var newModel = DoseResponseModelFactory.Create(modelType, newParameters, bmd, bmr);
            var newModelParameters = newModel.ExportParameters();
            Assert.AreEqual(newModelParameters["b"], originalParameters["b"], 1e-5);
        }

        /// <summary>
        /// Calculate parameter b from a, c and d using Weibull
        /// </summary>
        [TestMethod]
        public void DoseResponseModelFunction_TestComputeB_Weibull() {
            var a = .0569;
            var b = 77.59;
            var c = 0.6;
            var d = 1.2315;
            var sigma = 0.25;
            var s = 1;

            var originalParameters = new Dictionary<string, double>() {
                        { "a", a },
                        { "b", b },
                        { "c", c },
                        { "d", d },
                        { "sigma", sigma },
                        { "s", s },
                    };

            var newParameters = new Dictionary<string, double>() {
                        { "a", a },
                        { "c", c },
                        { "d", d },
                        { "sigma", sigma },
                        { "s", s },
                    };

            var bmd = 1.9;

            var modelType = DoseResponseModelType.Weibull;
            var originalModel = DoseResponseModelFactory.Create(modelType, originalParameters);
            var bmr = originalModel.Calculate(bmd);
            var newModel = DoseResponseModelFactory.Create(modelType, newParameters, bmd, bmr);
            var newModelParameters = newModel.ExportParameters();
            Assert.AreEqual(newModelParameters["b"], b, 1e-5);
        }

        /// <summary>
        /// Calculate parameter b from a, c and d using Gamma
        /// </summary>
        [TestMethod]
        public void DoseResponseModelFunction_TestComputeB_Gamma() {
            var a = .0238;
            var b = 0.006418;
            var c = 0.5476;
            var d = 1.2315;
            var sigma = 0.25;
            var s = 1;

            var originalParameters = new Dictionary<string, double>() {
                        { "a", a },
                        { "b", b },
                        { "c", c },
                        { "d", d },
                        { "sigma", sigma },
                        { "s", s },
                    };

            var newParameters = new Dictionary<string, double>() {
                        { "a", a },
                        { "c", c },
                        { "d", d },
                        { "sigma", sigma },
                        { "s", s },
                    };

            var bmd = 1.88871;

            var modelType = DoseResponseModelType.Gamma;
            var originalModel = DoseResponseModelFactory.Create(modelType, originalParameters);
            var bmr = originalModel.Calculate(bmd);
            var newModel = DoseResponseModelFactory.Create(modelType, newParameters, bmd, bmr);
            var newModelParameters = newModel.ExportParameters();
            Assert.AreEqual(newModelParameters["b"], b, 1e-5);
        }
        /// <summary>
        /// Calculate parameter b from a, c and d using LogProb
        /// </summary>
        [TestMethod]
        public void DoseResponseModelFunction_TestComputeB_LogProb() {
            var a = .02475;
            var b = 44.8294;
            var c = 0.4003;
            var d = 1.2315;
            var sigma = 0.25;
            var s = 1;

            var originalParameters = new Dictionary<string, double>() {
                        { "a", a },
                        { "b", b },
                        { "c", c },
                        { "d", d },
                        { "sigma", sigma },
                        { "s", s },
                    };

            var newParameters = new Dictionary<string, double>() {
                        { "a", a },
                        { "c", c },
                        { "d", d },
                        { "sigma", sigma },
                        { "s", s },
                    };

            var bmd = 1.82478;

            var modelType = DoseResponseModelType.LogProb;
            var originalModel = DoseResponseModelFactory.Create(modelType, originalParameters);
            var bmr = originalModel.Calculate(bmd);
            var newModel = DoseResponseModelFactory.Create(modelType, newParameters, bmd, bmr);
            var newModelParameters = newModel.ExportParameters();
            Assert.AreEqual(newModelParameters["b"], b, 1e-5);
        }
    }
}

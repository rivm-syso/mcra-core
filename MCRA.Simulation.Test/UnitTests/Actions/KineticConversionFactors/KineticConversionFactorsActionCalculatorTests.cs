using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.KineticConversionFactors;
using MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// KineticConversionFactorsActionCalculator tests.
    /// </summary>
    [TestClass]
    public class KineticConversionFactorsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the KineticConversionFactors action: load data, load default data, summarize
        /// action result, load data uncertain method
        /// </summary>
        [TestMethod]
        public void KineticConversionFactorActionCalculator_TestLoad() {
            var random = new McraRandomGenerator(1);

            var substances = FakeSubstancesGenerator.Create(1);
            var routes = new List<ExposureRoute>() { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver);
            var kineticConversionFactors = FakeKineticConversionFactorModelsGenerator
                .CreateKineticConversionFactors(
                    substances,
                    routes,
                    targetUnit
                );
            var compiledData = new CompiledData() {
                AllKineticConversionFactors = kineticConversionFactors
            };

            var project = new ProjectDto();
            var config = project.KineticModelsSettings;
            config.ExposureRoutes = routes;
            var data = new ActionData() {
                ActiveSubstances = substances
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);

            var calculator = new KineticConversionFactorsActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = [UncertaintySource.KineticConversionFactors]
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.KineticConversionFactors] = random
            };
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Runs compute KCF from PBK models action.
        /// </summary>
        [TestMethod]
        [DataRow(ExposureType.Acute)]
        [DataRow(ExposureType.Chronic)]
        public void KineticConversionFactorActionCalculator_TestCompute(ExposureType exposureType) {
            var random = new McraRandomGenerator(1);
            var filename = "Resources/PbkModels/EuroMixGenericPbk.sbml";
            var substances = FakeSubstancesGenerator.Create(1);
            var pbkModelInstances = substances
                .Select(r => FakeKineticModelsGenerator.CreateSbmlPbkModelInstance(r, filename, [("BM", 70D)]))
                .ToList();

            var project = new ProjectDto();
            var config = project.KineticConversionFactorsSettings;
            config.IsCompute = true;
            config.ExposureType = exposureType;
            config.ExposureRoutes = [ExposureRoute.Oral, ExposureRoute.Inhalation];
            config.InternalMatrices = [BiologicalMatrix.VenousBlood, BiologicalMatrix.Liver];
            config.NumberOfPbkModelSimulations = 10;
            config.ExposureRangeMinimum = 1;
            config.ExposureRangeMaximum = 1e2;
            config.NumberOfDays = 20;
            config.PbkSimulationMethod = PbkSimulationMethod.Standard;
            config.ExposureEventsGenerationMethod = ExposureEventsGenerationMethod.DailyAverageEvents;
            config.ComputeBetweenInternalTargetConversionFactors = true;

            var data = new ActionData() {
                ActiveSubstances = substances,
                KineticModelInstances = pbkModelInstances
            };

            var reportFileName = $"TestCompute_{exposureType}";
            var calculator = new KineticConversionFactorsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, reportFileName);

            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = [UncertaintySource.KineticConversionFactors]
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.KineticConversionFactors] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator,
                data,
                header,
                random,
                factorialSet,
                uncertaintySourceGenerators,
                reportFileName: reportFileName
            );
        }

        /// <summary>
        /// Runs compute KCF from PBK models action.
        /// </summary>
        [TestMethod]
        [DataRow(ExposureType.Acute, true)]
        [DataRow(ExposureType.Chronic, true)]
        [DataRow(ExposureType.Acute, false)]
        [DataRow(ExposureType.Chronic, false)]
        public void KineticConversionFactorActionCalculator_TestComputeFromIndividuals(ExposureType exposureType, bool bwAdjustment) {
            var random = new McraRandomGenerator(1);
            var filename = "Resources/PbkModels/simple_oral.sbml";
            var substances = FakeSubstancesGenerator.Create(1);
            var pbkModelInstances = substances
                .Select(r => FakeKineticModelsGenerator.CreateSbmlPbkModelInstance(r, filename))
                .ToList();

            var numIndividuals = 3;
            var individuals = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(numIndividuals, 2, false, random)
                .Cast<IIndividualDay>().ToList();
            foreach (var individual in individuals) {
                individual.SimulatedIndividual.BodyWeight = random.NextDouble(50, 90);
            }

            var project = new ProjectDto();
            var config = project.KineticConversionFactorsSettings;
            config.IsCompute = true;
            config.ExposureType = exposureType;
            config.ExposureRoutes = [ExposureRoute.Oral];
            config.InternalMatrices = [BiologicalMatrix.Blood];
            config.NumberOfPbkModelSimulations = 5;
            config.ExposureRangeMinimum = 1;
            config.ExposureRangeMaximum = 10;
            config.NumberOfDays = 20;
            config.PbkSimulationMethod = PbkSimulationMethod.Standard;
            config.ExposureEventsGenerationMethod = ExposureEventsGenerationMethod.DailyAverageEvents;
            config.ComputeBetweenInternalTargetConversionFactors = true;
            config.UseIndividuals = true;
            config.BodyWeightCorrected = bwAdjustment;

            var data = new ActionData() {
                ActiveSubstances = substances,
                KineticModelInstances = pbkModelInstances,
                Individuals = individuals
            };

            var bwAdjustmentStr = bwAdjustment ? "_bwAdjusted" : string.Empty;
            var reportFileName = $"TestComputeFromIndividuals_{exposureType}{bwAdjustmentStr}";
            var calculator = new KineticConversionFactorsActionCalculator(project);
            var (header, actionResult) = TestRunUpdateSummarizeNominal(project, calculator, data, reportFileName);

            var results = (actionResult as KineticConversionFactorsActionResult).KineticConversionFactorModels
                .Cast<KineticConversionFactorEmpiricalModel>()
                .ToList();
            Assert.HasCount(numIndividuals * config.NumberOfPbkModelSimulations, results.First().KineticConversionDataRecords);

            var factorialSet = new UncertaintyFactorialSet() {
                UncertaintySources = [UncertaintySource.KineticConversionFactors]
            };
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.KineticConversionFactors] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator,
                data,
                header,
                random,
                factorialSet,
                uncertaintySourceGenerators,
                reportFileName: reportFileName
            );
        }
    }
}
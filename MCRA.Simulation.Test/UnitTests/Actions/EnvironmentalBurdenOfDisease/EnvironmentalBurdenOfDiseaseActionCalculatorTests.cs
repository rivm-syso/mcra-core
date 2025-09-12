using CommandLine;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease;
using MCRA.Simulation.Calculators.CounterFactualValueModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    [TestClass]
    public class EnvironmentalBurdenOfDiseaseActionCalculatorTests : ActionCalculatorTestsBase {

        [TestMethod]
        public void EnvironmentalBurdenOfDiseaseActionCalculator_TestRunNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var project = new ProjectDto();
            project.ActionSettings.ExposureType = ExposureType.Chronic;
            var config = project.EnvironmentalBurdenOfDiseaseSettings;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.ExposureCalculationMethod = ExposureCalculationMethod.MonitoringConcentration;
            config.BodIndicators = [BodIndicator.DALY];
            var data = fakeActionData(random);
            var calculator = new EnvironmentalBurdenOfDiseaseActionCalculator(project);
            (var header, var result) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestRunNominal");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EnvironmentalBurdenOfDiseaseActionCalculator_TestRunUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            var project = new ProjectDto();
            project.ActionSettings.ExposureType = ExposureType.Chronic;
            var config = project.EnvironmentalBurdenOfDiseaseSettings;
            config.TargetDoseLevelType = TargetLevelType.Internal;
            config.ExposureCalculationMethod = ExposureCalculationMethod.MonitoringConcentration;
            config.BodIndicators = [BodIndicator.DALY];

            var data = fakeActionData(random);
            var calculator = new EnvironmentalBurdenOfDiseaseActionCalculator(project);

            (var header, var result) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestRunUncertain");

            var factorialSet = new UncertaintyFactorialSet(
                UncertaintySource.ExposureResponseFunctions
            );
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.ExposureResponseFunctions] = random
            };
            TestRunUpdateSummarizeUncertainty(
                calculator: calculator,
                data: data,
                header: header,
                random: random,
                factorialSet: factorialSet,
                uncertaintySources: uncertaintySourceGenerators,
                reportFileName: $"TestRunUncertain"
            );

            Assert.IsNotNull(result);
        }

        private static ActionData fakeActionData(McraRandomGenerator random) {
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Blood);
            var effects = FakeEffectsGenerator.Create(2);
            var substances = FakeSubstancesGenerator.Create(5);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var hbmIndividualConcentrations = FakeHbmDataGenerator
                .MockHumanMonitoringIndividualConcentrations(individuals, substances);
            var exposureResponseFunctionModels = FakeExposureResponseFunctionsGenerator
                .FakeExposureResponseFunctionModel(
                    substances,
                    effects,
                    targetUnit.Target,
                    targetUnit.ExposureUnit,
                    random.Next()
                );

            var counterFactualValueModels = FakeExposureResponseFunctionsGenerator
                .FakeCounterFactualValueModel(
                    exposureResponseFunctionModels
                );

            var burdenOfDiseases = effects.Select(r => FakeBurdenOfDiseasesGenerator.Create(r)).ToList();
            var populationCharacteristicTypes = exposureResponseFunctionModels
                .Select(r => r.ExposureResponseFunction.PopulationCharacteristic)
                .Distinct()
                .ToList();
            var population = FakePopulationsGenerator
                .Create(1, populationCharacteristicTypes, seed: 1).First();
            var data = new ActionData() {
                AllEffects = effects,
                SelectedPopulation = population,
                ExposureResponseFunctionModels = exposureResponseFunctionModels,
                CounterFactualValueModels = counterFactualValueModels,
                BurdensOfDisease = burdenOfDiseases,
                HbmIndividualCollections = [new() {
                    TargetUnit = targetUnit,
                        HbmIndividualConcentrations = hbmIndividualConcentrations
                    }
                ],
            };
            return data;
        }
    }
}

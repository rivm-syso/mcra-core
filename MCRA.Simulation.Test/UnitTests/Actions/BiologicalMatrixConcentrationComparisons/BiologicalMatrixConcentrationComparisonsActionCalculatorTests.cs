using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the HumanMonitoringAnalysis action
    /// </summary>
    [TestClass]
    public class BiologicalMatrixConcentrationComparisonsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the BiologicalMatrixConcentrationComparisons action:
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// </summary>
        [TestMethod]
        public void BiologicalMatrixConcentrationComparisonsActionCalculator_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var targetUnit = new TargetUnit(
                new ExposureTarget(BiologicalMatrix.Blood),
                new ExposureUnitTriple(
                    SubstanceAmountUnit.Micrograms,
                    ConcentrationMassUnit.Kilograms,
                    TimeScaleUnit.Peak
                )
            );

            var hbmIndividualDayConcentrations = FakeHbmIndividualDayConcentrationsGenerator
                .Create(individualDays, substances, samplingMethod, targetUnit, random);
            var hbmIndividualDayCumulativeConcentrations = FakeHbmCumulativeIndividualDayConcentrationsGenerator
                .Create(individualDays, random);

            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = MockKineticModelsGenerator
                .CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var targetExposuresCalculator = new InternalTargetExposuresCalculator(kineticModelCalculators);

            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individualDayTargetExposures = MockAggregateIndividualDayIntakeGenerator
                .Create(
                    individualDays,
                    substances,
                    new List<ExposurePathType>() { ExposurePathType.Dietary },
                    targetExposuresCalculator,
                    externalExposuresUnit,
                    random
                );

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.HumanMonitoringSettings.TargetMatrix = BiologicalMatrix.Blood;
            project.KineticModelSettings.CodeCompartment = "Blood";

            var data = new ActionData() {
                ActiveSubstances = substances,
                CorrectedRelativePotencyFactors = rpfs,
                ReferenceSubstance = substances.First(),
                MembershipProbabilities = substances.ToDictionary(c => c, c => 1d),
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                HbmIndividualDayCollections = hbmIndividualDayConcentrations,
                HbmCumulativeIndividualDayCollection = hbmIndividualDayCumulativeConcentrations,
                AggregateIndividualDayExposures = individualDayTargetExposures,
                TargetExposureUnit = targetUnit,
            };

            var calculator = new BiologicalMatrixConcentrationComparisonsActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "TestAcute");
            var result = calculator.Run(data, new CompositeProgressState());
        }

        /// <summary>
        /// Runs the BiologicalMatrixConcentrationComparisons action: 
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// </summary>
        [TestMethod]
        public void BiologicalMatrixConcentrationComparisonsActionCalculator_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(c => c, c => 1d);

            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod();
            var hbmCumulativeIndividualConcentrations = FakeHbmCumulativeIndividualConcentrationsGenerator
                .Create(individuals, random);
            var targetUnit = new TargetUnit(
                new ExposureTarget(BiologicalMatrix.Blood),
                new ExposureUnitTriple(
                    SubstanceAmountUnit.Micrograms,
                    ConcentrationMassUnit.Kilograms,
                    TimeScaleUnit.Peak
                )
            );
            var hbmIndividualConcentrations = FakeHbmIndividualConcentrationsGenerator
                .Create(individuals, substances, samplingMethod, targetUnit, random);

            var absorptionFactors = MockKineticModelsGenerator.CreateAbsorptionFactors(substances, 1);
            var kineticModelCalculators = MockKineticModelsGenerator.CreateAbsorptionFactorKineticModelCalculators(substances, absorptionFactors);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individualTargetExposures = MockAggregateIndividualIntakeGenerator.Create(
                individualDays,
                substances,
                new List<ExposurePathType>() { ExposurePathType.Dietary },
                kineticModelCalculators,
                externalExposuresUnit,
                random
            );

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.HumanMonitoringSettings.TargetMatrix = BiologicalMatrix.Blood;
            project.KineticModelSettings.CodeCompartment = "Blood";

            var data = new ActionData() {
                ActiveSubstances = substances,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = rpfs,
                HbmIndividualCollections =  hbmIndividualConcentrations,
                HbmCumulativeIndividualCollection = hbmCumulativeIndividualConcentrations,
                HbmSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                AggregateIndividualExposures = individualTargetExposures,
                MembershipProbabilities = substances.ToDictionary(c => c, c => 1d),
                TargetExposureUnit = targetUnit,
            };

            var calculator = new BiologicalMatrixConcentrationComparisonsActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, "TestChronic");
            var result = calculator.Run(data, new CompositeProgressState());
        }
    }
}

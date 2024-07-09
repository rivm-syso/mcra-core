using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, KineticModel
    /// </summary>
    [TestClass]
    public class KineticModelTimeCourseChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart and test KineticModelTimeCourseChartCreator view, chronic
        /// </summary>
        [TestMethod]
        public void KineticModelTimeCourseChartCreatorTests_TestLongTerm() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);
            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 100;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeChronic(
                    individualExposures,
                    substances,
                    routes,
                    exposureUnit,
                    new List<TargetUnit>() { targetUnit },
                    random,
                    new ProgressState()
                );
            var section = new KineticModelTimeCourseSection();
            section.Summarize(
                targetExposures,
                routes.Select(r => r.GetExposureRoute()).ToList(),
                substance,
                instance,
                new List<TargetUnit>() { targetUnit },
                exposureUnit,
                ExposureType.Chronic
            );

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelTimeCourseChartCreator_TestLongTerm");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new KineticModelTimeCourseChartCreator(record, section, exposureUnit.SubstanceAmountUnit.ToString());
                RenderChart(chart, $"TestCreate{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, acute
        /// </summary>
        [TestMethod]
        public void InternalVersusExternalExposuresSection_TestPeak() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 130;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeAcute(
                    individualDayExposures,
                    substances,
                    routes,
                    externalExposuresUnit,
                    new List<TargetUnit>() { targetUnit },
                    random,
                    new ProgressState()
                );
            var individualIds = KineticModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    targetExposures,
                    substances,
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    targetUnit
                );
            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividualId))
                .Cast<AggregateIndividualExposure>()
                .ToList();

            var section = new KineticModelTimeCourseSection();
            section.Summarize(
                drillDownRecords,
                routes.Select(r => r.GetExposureRoute()).ToList(),
                substance,
                instance,
                new List<TargetUnit>() { targetUnit },
                externalExposuresUnit,
                ExposureType.Acute
            );

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelTimeCourseChartCreator_TestPeak");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new KineticModelTimeCourseChartCreator(record, section, externalExposuresUnit.SubstanceAmountUnit.ToString());
                RenderChart(chart, $"TestCreate2{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, single dose acute
        /// </summary>
        [TestMethod]
        [TestCategory("Documentation charts")]
        public void KineticModelTimeCourseChartCreator_TestSingleIndividualDayExposure() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] {
                ExposurePathType.Oral,
                ExposurePathType.Dermal,
                ExposurePathType.Inhalation
            };

            var individual = new Individual(0) { BodyWeight = 70D };
            var individualDayExposure = ExternalIndividualDayExposure.FromSingleDose(
                ExposurePathType.Oral,
                substance,
                0.01,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerGBWPerDay),
                individual
            );

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 50;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeAcute(
                    new List<IExternalIndividualDayExposure>() { individualDayExposure },
                    substances,
                    routes,
                    exposureUnit,
                    new List<TargetUnit>() { targetUnit },
                    random,
                    new ProgressState()
                );

            var individualIds = KineticModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    targetExposures.Cast<AggregateIndividualExposure>().ToList(),
                    substances,
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    targetUnit
                );
            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividualId))
                .Cast<AggregateIndividualExposure>()
                .ToList();
            var section = new KineticModelTimeCourseSection();
            section.Summarize(
                drillDownRecords,
                routes.Select(r => r.GetExposureRoute()).ToList(),
                substance,
                instance,
                new List<TargetUnit>() { targetUnit },
                exposureUnit,
                ExposureType.Acute
            );

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new KineticModelTimeCourseChartCreator(record, section, exposureUnit.SubstanceAmountUnit.GetShortDisplayName());
                RenderChart(chart, $"TestCreateAcute{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, single dose chronic
        /// </summary>
        [TestMethod]
        [TestCategory("Documentation charts")]
        public void KineticModelTimeCourseChartCreator_TestSingleIndividualExposure() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var individual = new Individual(0) { BodyWeight = 70D };
            var exposureDay1 = ExternalIndividualDayExposure.FromSingleDose(ExposurePathType.Oral, substance, 0.01, exposureUnit, individual);
            var exposureDay2 = ExternalIndividualDayExposure.FromSingleDose(ExposurePathType.Oral, substance, 0.05, exposureUnit, individual);
            var individualExposure = new ExternalIndividualExposure() {
                Individual = individual,
                ExternalIndividualDayExposures = new List<IExternalIndividualDayExposure>() { exposureDay1, exposureDay2 }
            };

            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerKg, BiologicalMatrix.Liver);

            var instance = MockKineticModelsGenerator.CreatePbkModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 50;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeChronic(
                    new List<IExternalIndividualExposure>() { individualExposure },
                    substances,
                    routes,
                    exposureUnit,
                    new List<TargetUnit>() { targetUnit },
                    random,
                    new ProgressState()
                );
            var individualIds = KineticModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    targetExposures,
                    substances,
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    targetUnit
                );

            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividualId))
                .ToList();
            var section = new KineticModelTimeCourseSection();
            section.Summarize(
                drillDownRecords,
                routes.Select(r => r.GetExposureRoute()).ToList(),
                substance,
                instance,
                new List<TargetUnit>() { targetUnit },
                exposureUnit,
                ExposureType.Chronic
            );

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new KineticModelTimeCourseChartCreator(record, section, targetUnit.SubstanceAmountUnit.GetShortDisplayName());
                RenderChart(chart, $"TestCreateChronic{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }
    }
}

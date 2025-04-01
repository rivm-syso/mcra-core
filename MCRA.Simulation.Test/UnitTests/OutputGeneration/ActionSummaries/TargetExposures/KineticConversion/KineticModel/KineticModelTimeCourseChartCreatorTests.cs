using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
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
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var paths = FakeExposurePathGenerator.Create(routes);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);
            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() { 
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = 1
            };

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, simulationSettings) }
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
                    [targetUnit],
                    random,
                    new ProgressState()
                );
            var section = new PbkModelTimeCourseSection();
            section.Summarize(
                targetExposures,
                routes,
                substance,
                instance,
                [targetUnit],
                exposureUnit,
                ExposureType.Chronic,
                simulationSettings.NonStationaryPeriod
            );

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelTimeCourseChartCreator_TestLongTerm");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, exposureUnit.SubstanceAmountUnit.ToString());
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
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var paths = FakeExposurePathGenerator.Create(routes);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);

            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = 1
            };

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, simulationSettings) }
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
                    [targetUnit],
                    random,
                    new ProgressState()
                );
            var individualIds = PbkModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    targetExposures,
                    substances,
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    targetUnit
                );
            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividual.Id))
                .Cast<AggregateIndividualExposure>()
                .ToList();

            var section = new PbkModelTimeCourseSection();
            section.Summarize(
                drillDownRecords,
                routes,
                substance,
                instance,
                [targetUnit],
                externalExposuresUnit,
                ExposureType.Acute,
                simulationSettings.NonStationaryPeriod
            );

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelTimeCourseChartCreator_TestPeak");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, externalExposuresUnit.SubstanceAmountUnit.ToString());
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
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] {
                ExposureRoute.Oral,
                ExposureRoute.Dermal,
                ExposureRoute.Inhalation
            };
            var paths = FakeExposurePathGenerator.Create(routes);

            var individual = new SimulatedIndividual(new(0) { BodyWeight = 70D }, 0);
            var individualDayExposure = ExternalIndividualDayExposure.FromSingleDose(
                ExposureRoute.Oral,
                substance,
                0.01,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerGBWPerDay),
                individual
            );

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() { 
                NumberOfSimulatedDays = 100,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = 50
            };

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, simulationSettings) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeAcute(
                    [individualDayExposure],
                    substances,
                    routes,
                    exposureUnit,
                    [targetUnit],
                    random,
                    new ProgressState()
                );

            var individualIds = PbkModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    targetExposures.Cast<AggregateIndividualExposure>().ToList(),
                    substances,
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    targetUnit
                );
            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividual.Id))
                .Cast<AggregateIndividualExposure>()
                .ToList();
            var section = new PbkModelTimeCourseSection();
            section.Summarize(
                drillDownRecords,
                routes,
                substance,
                instance,
                [targetUnit],
                exposureUnit,
                ExposureType.Acute,
                simulationSettings.NonStationaryPeriod
            );

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, exposureUnit.SubstanceAmountUnit.GetShortDisplayName());
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
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var paths = FakeExposurePathGenerator.Create(routes);

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var individual = new SimulatedIndividual(new(0) { BodyWeight = 70D }, 0);
            var exposureDay1 = ExternalIndividualDayExposure.FromSingleDose(ExposureRoute.Oral, substance, 0.01, exposureUnit, individual);
            var exposureDay2 = ExternalIndividualDayExposure.FromSingleDose(ExposureRoute.Oral, substance, 0.05, exposureUnit, individual);
            var individualExposure = new ExternalIndividualExposure(individual) {
                ExternalIndividualDayExposures = [exposureDay1, exposureDay2]
            };

            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerKg, BiologicalMatrix.Liver);

            var instance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 100,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = 50
            };

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, simulationSettings) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeChronic(
                    [individualExposure],
                    substances,
                    routes,
                    exposureUnit,
                    [targetUnit],
                    random,
                    new ProgressState()
                );
            var individualIds = PbkModelTimeCourseSection
                 .GetDrilldownIndividualIds(
                    targetExposures,
                    substances,
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    targetUnit
                );

            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividual.Id))
                .ToList();
            var section = new PbkModelTimeCourseSection();
            section.Summarize(
                drillDownRecords,
                routes,
                substance,
                instance,
                [targetUnit],
                exposureUnit,
                ExposureType.Chronic,
                simulationSettings.NonStationaryPeriod
            );

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, targetUnit.SubstanceAmountUnit.GetShortDisplayName());
                RenderChart(chart, $"TestCreateChronic{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, single dose chronic
        /// </summary>
        [TestMethod]
        [DataRow(TimeUnit.Minutes)]
        [DataRow(TimeUnit.Hours)]
        [DataRow(TimeUnit.Days)]
        public void KineticModelTimeCourseChartCreator_TestCreateTimeScales(TimeUnit timeUnit) {
            var timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(timeUnit);
            List<(int time, double exposure)> timeSeries = Enumerable
                .Range(0, 50 * (int)timeUnitMultiplier)
                .Select(r => (r, Math.Sin(r / timeUnitMultiplier / 6 * Math.PI)))
                .ToList();
            var section = new PbkModelTimeCourseSection() {
                TimeScale = timeUnit,
                NumberOfDaysSkipped = 10,
                EvaluationFrequency = 1,
                InternalTargetSystemExposures = [
                    new PbkModelTimeCourseDrilldownRecord() {
                        TargetExposures = timeSeries
                            .Select(r => new TargetIndividualExposurePerTimeUnitRecord() {
                                Time = r.time,
                                Exposure = r.exposure
                            })
                            .ToList(),
                        RatioInternalExternal = .8
                    }
                ]
            };
            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelTimeCourseChartCreator_TestLongTerm");
            var record = section.InternalTargetSystemExposures.First();
            var chart = new PbkModelTimeCourseChartCreator(
                record,
                section,
                "INTAKEUNIT"
            );
            RenderChart(chart, $"TestCreateTimeScales_{timeUnit}");
        }
    }
}

using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    [TestClass]
    public class PbkModelTimeCourseChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart and test KineticModelTimeCourseChartCreator view, chronic
        /// </summary>
        [TestMethod]
        public void PbkModelTimeCourseChartCreator_TestLongTerm() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var paths = FakeExposurePathGenerator.Create(routes);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = FakeExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, paths, seed);
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            // TODO: refactor to not do all the simulation here in order to get results for the chart creation
            var nonStationaryPeriod = 1;
            var modelInstance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = nonStationaryPeriod
            };
            var pbkModelCalculator = new CosmosKineticModelCalculator(modelInstance, simulationSettings);
            var pbkKineticConversionCalculator = new PbkKineticConversionCalculator(pbkModelCalculator);
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(
                new Dictionary<Compound, IKineticConversionCalculator>() {
                    { substance, pbkKineticConversionCalculator }
                }
            );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);

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
                [targetUnit],
                exposureUnit,
                nonStationaryPeriod: nonStationaryPeriod
            );

            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, exposureUnit.SubstanceAmountUnit.ToString());
                RenderChart(chart, $"PbkModelTimeCourseChartCreator_TestCreate_{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, acute
        /// </summary>
        [TestMethod]
        public void PbkModelTimeCourseChartCreator_TestPeak() {
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
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            var nonStationaryPeriod = 1;
            var modelInstance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = nonStationaryPeriod
            };
            var pbkModelCalculator = new CosmosKineticModelCalculator(modelInstance, simulationSettings);
            var pbkKineticConversionCalculator = new PbkKineticConversionCalculator(pbkModelCalculator);
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(
                new Dictionary<Compound, IKineticConversionCalculator>() {
                    { substance, pbkKineticConversionCalculator }
                }
            );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);

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
                [targetUnit],
                externalExposuresUnit,
                nonStationaryPeriod
            );

            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, externalExposuresUnit.SubstanceAmountUnit.ToString());
                RenderChart(chart, $"PbkModelTimeCourseChartCreator_TestPeak_{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, single dose acute
        /// </summary>
        [TestMethod]
        [TestCategory("Documentation charts")]
        public void PbkModelTimeCourseChartCreator_TestSingleIndividualDayExposure() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] {
                ExposureRoute.Oral,
                ExposureRoute.Dermal,
                ExposureRoute.Inhalation
            };

            var individual = new SimulatedIndividual(new(0) { BodyWeight = 70D }, 0);
            var individualDayExposure = ExternalIndividualDayExposure.FromSingleDose(
                ExposureRoute.Oral,
                substance,
                0.01,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerGBWPerDay),
                individual
            );

            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);

            // TODO: refactor to not do all the simulation here in order to get results for the section summarization
            var nonStationaryPeriod = 1;
            var modelInstance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = nonStationaryPeriod
            };
            var pbkModelCalculator = new CosmosKineticModelCalculator(modelInstance, simulationSettings);
            var pbkKineticConversionCalculator = new PbkKineticConversionCalculator(pbkModelCalculator);
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(
                new Dictionary<Compound, IKineticConversionCalculator>() {
                    { substance, pbkKineticConversionCalculator }
                }
            );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(
                kineticConversionCalculatorProvider
            );

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

            var drillDownRecords = targetExposures
                .Cast<AggregateIndividualExposure>()
                .ToList();
            var section = new PbkModelTimeCourseSection();
            section.Summarize(
                drillDownRecords,
                routes,
                substance,
                [targetUnit],
                exposureUnit,
                nonStationaryPeriod
            );

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, exposureUnit.SubstanceAmountUnit.GetShortDisplayName());
                RenderChart(chart, $"PbkModelTimeCourseChartCreator_TestCreateAcute_{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, single dose chronic
        /// </summary>
        [TestMethod]
        [TestCategory("Documentation charts")]
        public void PbkModelTimeCourseChartCreator_TestSingleIndividualExposure() {
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

            // TODO: refactor to not do all the simulation here in order to get results for the section summarization
            var nonStationaryPeriod = 1;
            var modelInstance = FakeKineticModelsGenerator.CreatePbkModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 5,
                UseRepeatedDailyEvents = true,
                NumberOfOralDosesPerDay = 1,
                NonStationaryPeriod = nonStationaryPeriod
            };
            var pbkModelCalculator = new CosmosKineticModelCalculator(modelInstance, simulationSettings);
            var pbkKineticConversionCalculator = new PbkKineticConversionCalculator(pbkModelCalculator);
            var kineticConversionCalculatorProvider = new KineticConversionCalculatorProvider(
                new Dictionary<Compound, IKineticConversionCalculator>() {
                    { substance, pbkKineticConversionCalculator }
                }
            );
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(kineticConversionCalculatorProvider);

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
                [targetUnit],
                exposureUnit,
                nonStationaryPeriod
            );

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PbkModelTimeCourseChartCreator(record, section, targetUnit.SubstanceAmountUnit.GetShortDisplayName());
                RenderChart(chart, $"PbkModelTimeCourseChartCreator_TestCreateChronic_{record.IndividualCode}");
            }
            AssertIsValidView(section);
        }

        [TestMethod]
        [DataRow(TimeUnit.Minutes)]
        [DataRow(TimeUnit.Hours)]
        [DataRow(TimeUnit.Days)]
        public void PbkModelTimeCourseChartCreator_TestCreateTimeScales(TimeUnit timeUnit) {
            var timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(timeUnit);
            List<(int time, double exposure)> timeSeries = Enumerable
                .Range(0, 50 * (int)timeUnitMultiplier)
                .Select(r => (r, Math.Sin(r / timeUnitMultiplier / 6 * Math.PI)))
                .ToList();
            var section = new PbkModelTimeCourseSection() {
                NumberOfDaysSkipped = 10,
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
            RenderChart(chart, $"PbkModelTimeCourseChartCreator_TestCreateTimeScales_{timeUnit}");
        }
    }
}

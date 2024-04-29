using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
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
    public class KineticModelSectionTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, chronic
        /// </summary>
        [TestMethod]
        public void KineticModelSectionTests_TestLongTerm() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);
            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);

            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "CLiver" };
            instance.NonStationaryPeriod = 100;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualExposures(
                    individualExposures,
                    substances,
                    substance,
                    routes,
                    exposureUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                )
                .First().TargetIndividualExposures.Cast<ITargetExposure>().ToList();
            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDrillDown(targetExposures, routes, substance, instance, null, ExposureType.Chronic);

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelSectionTests_TestLongTerm");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, exposureUnit.SubstanceAmountUnit.ToString());
                RenderChart(chart, $"TestCreate1{record.Code}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, acute
        /// </summary>
        [TestMethod]
        public void KineticModelSectionTests_TestPeak() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "CLiver" };
            instance.NonStationaryPeriod = 130;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
                    individualDayExposures,
                    substances,
                    substance,
                    routes,
                    targetUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                )
                .First().TargetIndividualDayExposures;
            var individualIds = KineticModelTimeCourseSection
                 .GetTargetExposuresIds(
                    targetExposures.Cast<ITargetExposure>().ToList(),
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    false
                );
            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividualId))
                .Cast<ITargetExposure>()
                .ToList();

            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDrillDown(drillDownRecords, routes, substance, instance, null, ExposureType.Acute);

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelSectionTests_TestPeak");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, targetUnit.SubstanceAmountUnit.ToString());
                RenderChart(chart, $"TestCreate2{record.Code}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, single dose acute
        /// </summary>
        [TestMethod]
        [TestCategory("Documentation charts")]
        public void KineticModelSectionTests_TestSingleIndividualDayExposure() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() {
                ExposurePathType.Dietary,
                ExposurePathType.Oral,
                ExposurePathType.Dermal,
                ExposurePathType.Inhalation
            };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);

            var individual = new Individual(0) { BodyWeight = 70D };
            var individualDayExposure = ExternalIndividualDayExposure.FromSingleDose(
                ExposurePathType.Dietary,
                substance,
                0.01,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerGBWPerDay),
                individual
            );

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv6KineticModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "CLiver" };
            instance.NonStationaryPeriod = 50;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
                    new List<IExternalIndividualDayExposure>() { individualDayExposure },
                    substances,
                    substance,
                    routes,
                    exposureUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                )
                .First().TargetIndividualDayExposures;

            var individualIds = KineticModelTimeCourseSection
                 .GetTargetExposuresIds(
                    targetExposures.Cast<ITargetExposure>().ToList(),
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    false
                );
            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividualId))
                .Cast<ITargetExposure>()
                .ToList();
            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDrillDown(drillDownRecords, routes, substance, instance, null, ExposureType.Acute);

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, exposureUnit.SubstanceAmountUnit.GetShortDisplayName());
                RenderChart(chart, $"TestCreate3{record.Code}");
            }
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test KineticModelTimeCourseSection view, single dose chronic
        /// </summary>
        [TestMethod]
        [TestCategory("Documentation charts")]
        public void KineticModelSectionTests_TestSingleIndividualExposure() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var individual = new Individual(0) { BodyWeight = 70D };
            var exposureDay1 = ExternalIndividualDayExposure.FromSingleDose(ExposurePathType.Dietary, substance, 0.01, exposureUnit, individual);
            var exposureDay2 = ExternalIndividualDayExposure.FromSingleDose(ExposurePathType.Dietary, substance, 0.05, exposureUnit, individual);
            var individualExposure = new ExternalIndividualExposure() {
                Individual = individual,
                ExternalIndividualDayExposures = new List<IExternalIndividualDayExposure>() { exposureDay1, exposureDay2 }
            };

            var compartment = "CLiver";
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerKg, BiologicalMatrixConverter.FromString(compartment));

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv6KineticModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { compartment };
            instance.NonStationaryPeriod = 50;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualExposures(
                    new List<IExternalIndividualExposure>() { individualExposure },
                    substances,
                    substance,
                    routes,
                    exposureUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                )
                .First().TargetIndividualExposures;
            var individualIds = KineticModelTimeCourseSection
                 .GetTargetExposuresIds(
                    targetExposures.Cast<ITargetExposure>().ToList(),
                    substances.ToDictionary(r => r, r => .1),
                    substances.ToDictionary(r => r, r => .1),
                    95,
                    false
                );

            var drillDownRecords = targetExposures
                .Where(c => individualIds.Contains(c.SimulatedIndividualId))
                .Cast<ITargetExposure>()
                .ToList();
            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDrillDown(drillDownRecords, routes, substance, instance, null, ExposureType.Chronic);

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, targetUnit.SubstanceAmountUnit.GetShortDisplayName());
                RenderChart(chart, $"TestCreate4{record.Code}");
            }
            AssertIsValidView(section);
        }
        /// <summary>
        /// Create chart and test KineticModelSection view, chronic, absorptionfactors
        /// </summary>
        [TestMethod]
        public void KineticModelSectionTests_AbsorptionFactorsChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "CLiver" };
            instance.NonStationaryPeriod = 100;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);

            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualExposures(
                    individualExposures,
                    substances,
                    substance,
                    routes,
                    exposureUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                ).First();

            var targetExposures = new List<ITargetExposure>();
            foreach (var item in targetIndividualExposures.TargetIndividualExposures) {
                var targetExposure = new AggregateIndividualExposure() {
                    TargetExposuresBySubstance = item.TargetExposuresBySubstance,
                    RelativeCompartmentWeight = item.RelativeCompartmentWeight,
                    Individual = item.Individual,
                    IndividualSamplingWeight = item.IndividualSamplingWeight,
                    ExternalIndividualDayExposures = individualExposures
                        .First(c => c.SimulatedIndividualId == item.SimulatedIndividualId).ExternalIndividualDayExposures,
                };
                targetExposures.Add(targetExposure);
            }
            var absorptionFactorsPerCompound = absorptionFactors.ToDictionary(c => (c.Key, substance), c => c.Value);
            var section = new KineticModelSection();
            section.SummarizeAbsorptionFactors(absorptionFactorsPerCompound, substance, routes);
            section.SummarizeAbsorptionChart(targetExposures, substance, routes, ExposureType.Chronic, instance.CompartmentCodes);

            var chart = new KineticModelChartCreator(section, "CLiver", "mg/kg", "mg/kg bw/day");
            RenderChart(chart, $"TestCreate4");
            AssertIsValidView(section);
        }
        /// <summary>
        /// Create chart and test KineticModelSection view, acute, absorptionfactors
        /// </summary>
        [TestMethod]
        public void KineticModelSectionTests_AbsorptionFactorsAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "CLiver" };
            instance.NonStationaryPeriod = 100;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);

            var exposureUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
                    individualDayExposures,
                    substances,
                    substance,
                    routes,
                    exposureUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                ).First();

            var targetExposures = new List<ITargetExposure>();
            foreach (var item in targetIndividualExposures.TargetIndividualDayExposures) {
                var aggregateIndividualDayExposure = new AggregateIndividualDayExposure() {
                    TargetExposuresBySubstance = item.TargetExposuresBySubstance,
                    RelativeCompartmentWeight = item.RelativeCompartmentWeight,
                    Individual = item.Individual,
                    IndividualSamplingWeight = item.IndividualSamplingWeight,
                    ExposuresPerRouteSubstance = individualDayExposures
                        .First(c => c.SimulatedIndividualDayId == item.SimulatedIndividualDayId).ExposuresPerRouteSubstance,
                };
                targetExposures.Add(aggregateIndividualDayExposure);
            }

            var absorptionFactorsPerCompound = absorptionFactors.ToDictionary(c => (c.Key, substance), c => c.Value);
            var section = new KineticModelSection();
            section.SummarizeAbsorptionFactors(absorptionFactorsPerCompound, substance, routes);
            section.SummarizeAbsorptionChart(targetExposures, substance, routes, ExposureType.Acute, instance.CompartmentCodes);

            var chart = new KineticModelChartCreator(section, "CLiver", "mg/kg", "mg/kg bw/day");
            RenderChart(chart, $"TestCreate6");
            AssertIsValidView(section);
        }
    }
}

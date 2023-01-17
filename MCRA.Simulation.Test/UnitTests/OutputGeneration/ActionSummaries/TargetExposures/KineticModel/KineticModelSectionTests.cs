using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Collections;
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
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Oral, ExposureRouteType.Dermal, ExposureRouteType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            var compartment = "CLiver";
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = compartment;
            instance.NonStationaryPeriod = 100;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetUnit = TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, compartment);
            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualExposures(
                    individualExposures,
                    substances,
                    substance,
                    routes,
                    targetUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                )
                .Cast<ITargetIndividualExposure>()
                .ToList();
            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDrillDown(targetIndividualExposures, routes, substance, instance, false);

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelSectionTests_TestLongTerm");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, targetUnit.SubstanceAmount.ToString());
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
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Oral, ExposureRouteType.Dermal, ExposureRouteType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            var compartment = "CLiver";
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = compartment;
            instance.NonStationaryPeriod = 130;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetUnit = TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, compartment);
            var targetIndividualDayExposures = internalTargetExposuresCalculator
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
                .Cast<ITargetIndividualDayExposure>()
                .ToList();

            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDayDrillDown(targetIndividualDayExposures, routes, substance, instance);

            var outputPath = TestUtilities.CreateTestOutputPath("KineticModelSectionTests_TestPeak");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, targetUnit.SubstanceAmount.ToString());
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
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Oral, ExposureRouteType.Dermal, ExposureRouteType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);

            var compartment = "CLiver";
            var targetUnit = TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, compartment);

            var individual = new Individual(0) { BodyWeight = 70D };
            var individualDayExposure = ExternalIndividualDayExposure.FromSingleDose(ExposureRouteType.Dietary, substance, 0.01, targetUnit, individual);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv6KineticModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = compartment;
            instance.NonStationaryPeriod = 50;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetIndividualDayExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
                    new List<IExternalIndividualDayExposure>() { individualDayExposure },
                    substances,
                    substance,
                    routes,
                    targetUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                )
                .Cast<ITargetIndividualDayExposure>()
                .ToList();

            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDayDrillDown(targetIndividualDayExposures, routes, substance, instance);

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, targetUnit.SubstanceAmount.GetShortDisplayName());
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
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Oral, ExposureRouteType.Dermal, ExposureRouteType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);

            var compartment = "CLiver";
            var targetUnit = TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, compartment);

            var individual = new Individual(0) { BodyWeight = 70D };
            var exposureDay1 = ExternalIndividualDayExposure.FromSingleDose(ExposureRouteType.Dietary, substance, 0.01, targetUnit, individual);
            var exposureDay2 = ExternalIndividualDayExposure.FromSingleDose(ExposureRouteType.Dietary, substance, 0.05, targetUnit, individual);
            var individualExposure = new ExternalIndividualExposure() {
                Individual = individual,
                ExternalIndividualDayExposures = new List<IExternalIndividualDayExposure>() { exposureDay1, exposureDay2 }
            };

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv6KineticModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = compartment;
            instance.NonStationaryPeriod = 50;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualExposures(
                    new List<IExternalIndividualExposure>() { individualExposure },
                    substances,
                    substance,
                    routes,
                    targetUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                )
                .Cast<ITargetIndividualExposure>()
                .ToList();

            var section = new KineticModelTimeCourseSection();
            section.SummarizeIndividualDrillDown(targetIndividualExposures, routes, substance, instance, false);

            var outputPath = TestUtilities.GetOrCreateTestOutputPath("Documentation");
            foreach (var record in section.InternalTargetSystemExposures) {
                var chart = new PBPKChartCreator(record, section, targetUnit.SubstanceAmount.GetShortDisplayName());
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
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Oral, ExposureRouteType.Dermal, ExposureRouteType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            var compartment = "CLiver";
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = compartment;
            instance.NonStationaryPeriod = 100;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetUnit = TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, compartment);
            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualExposures(
                    individualExposures,
                    substances,
                    substance,
                    routes,
                    targetUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                ).ToList();

            var aggregateIndividualExposures = new List<AggregateIndividualExposure>();
            foreach (var item in targetIndividualExposures) {
                var aggregateIndividualExposure = new AggregateIndividualExposure() {
                    TargetExposuresBySubstance = item.TargetExposuresBySubstance,
                    RelativeCompartmentWeight = item.RelativeCompartmentWeight,
                    Individual = item.Individual,
                    IndividualSamplingWeight = item.IndividualSamplingWeight,
                    ExternalIndividualDayExposures = individualExposures
                        .Where(c => c.SimulatedIndividualId == item.SimulatedIndividualId)
                        .First().ExternalIndividualDayExposures,
                };
                aggregateIndividualExposures.Add(aggregateIndividualExposure);
            }

            var absorptionFactorsPerCompound = new TwoKeyDictionary<ExposureRouteType, Compound, double>();
            foreach (var item in absorptionFactors) {
                absorptionFactorsPerCompound[item.Key, substance] = item.Value;
            }
            var section = new KineticModelSection();
            section.SummarizeAbsorptionFactors(absorptionFactorsPerCompound, substance, routes);

            section.SummarizeAbsorptionChart(aggregateIndividualExposures, substance, routes);

            var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
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
            var routes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Oral, ExposureRouteType.Dermal, ExposureRouteType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            var compartment = "CLiver";
            instance.NumberOfDays = 5;
            instance.NumberOfDosesPerDay = 1;
            instance.CodeCompartment = compartment;
            instance.NonStationaryPeriod = 100;

            var models = new Dictionary<Compound, IKineticModelCalculator>() {
                { substance, new CosmosKineticModelCalculator(instance, absorptionFactors) }
            };
            var internalTargetExposuresCalculator = new InternalTargetExposuresCalculator(models);
            var targetUnit = TargetUnit.FromDoseUnit(DoseUnit.mgPerKg, compartment);
            var targetIndividualExposures = internalTargetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
                    individualDayExposures,
                    substances,
                    substance,
                    routes,
                    targetUnit,
                    random,
                    new List<KineticModelInstance>() { instance },
                    new ProgressState()
                ).ToList();

            var aggregateIndividualDayExposures = new List<AggregateIndividualDayExposure>();
            foreach (var item in targetIndividualExposures) {
                var aggregateIndividualDayExposure = new AggregateIndividualDayExposure() {
                    TargetExposuresBySubstance = item.TargetExposuresBySubstance,
                    RelativeCompartmentWeight = item.RelativeCompartmentWeight,
                    Individual = item.Individual,
                    IndividualSamplingWeight = item.IndividualSamplingWeight,
                    ExposuresPerRouteSubstance = individualDayExposures
                        .Where(c => c.SimulatedIndividualDayId == item.SimulatedIndividualDayId)
                        .First().ExposuresPerRouteSubstance,
                };
                aggregateIndividualDayExposures.Add(aggregateIndividualDayExposure);
            }

            var absorptionFactorsPerCompound = new TwoKeyDictionary<ExposureRouteType, Compound, double>();
            foreach (var item in absorptionFactors) {
                absorptionFactorsPerCompound[item.Key, substance] = item.Value;
            }
            var section = new KineticModelSection();
            section.SummarizeAbsorptionFactors(absorptionFactorsPerCompound, substance, routes);

            section.SummarizeAbsorptionChart(aggregateIndividualDayExposures, substance, routes);

            var chart = new KineticModelChartCreator(section, "mg/kg", "mg/kg bw/day");
            RenderChart(chart, $"TestCreate6");
            AssertIsValidView(section);
        }
    }
}

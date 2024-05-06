using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using DocumentFormat.OpenXml.Bibliography;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class CosmosKineticModelCalculatorTests {

        /// <summary>
        /// CosmosModelCalculator: calculates individual day target exposures, CosmosV4, acute
        /// </summary>
        [TestMethod]
        public void CosmosModelCalculator_TestCalculateIndividualDayTargetExposuresCosmosV4() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "CLiver" };

            var model = new CosmosKineticModelCalculator(instance, absorptionFactors);

            var internalExposures = model
                .CalculateIndividualDayTargetExposures(
                    individualDayExposures,
                    substance,
                    routes,
                    ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                    new List<TargetUnit> { TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL) },
                    new ProgressState(),
                    random
                )
                .First();

            Assert.AreEqual(50, internalExposures.IndividualDaySubstanceTargetExposures.Count);
            var positiveExternalExposures = individualDayExposures.Where(r => r.ExposuresPerRouteSubstance.Any(eprc => eprc.Value.Any(ipc => ipc.Exposure > 0)));
            Assert.AreEqual(positiveExternalExposures.Count(), internalExposures.IndividualDaySubstanceTargetExposures.SelectMany(r => r.SubstanceTargetExposures).Count(c => c.SubstanceAmount > 0));
            Assert.AreEqual(100 * 24 + 1, internalExposures.IndividualDaySubstanceTargetExposures.SelectMany(r => r.SubstanceTargetExposures).Select(c => (SubstanceTargetExposurePattern)c).First().TargetExposuresPerTimeUnit.Count);
        }

        /// <summary>
        /// CosmosModelCalculator: calculates individual  target exposures, CosmosV4, chronic
        /// </summary>
        [TestMethod]
        public void CosmosModelCalculator_TestCalculateIndividualTargetExposuresCosmosV4() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Dietary, ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var absorptionFactors = routes.ToDictionary(r => r, r => .1);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);

            var instance = MockKineticModelsGenerator.CreateFakeEuroMixPBTKv5KineticModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.CompartmentCodes = new List<string> { "CLiver" };
            instance.NonStationaryPeriod = 10;

            var kineticModelCalculator = new CosmosKineticModelCalculator(instance, absorptionFactors);
            var internalExposures = kineticModelCalculator.CalculateIndividualTargetExposures(
                individualExposures,
                substance,
                routes,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                new List<TargetUnit> { TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL) },
                new ProgressState(),
                random
            ).First();

            Assert.AreEqual(25, internalExposures.IndividualSubstanceTargetExposures.Count);
            var positiveExternalExposures = individualExposures
                .Where(r => r.ExternalIndividualDayExposures
                    .Any(eide => eide.ExposuresPerRouteSubstance.Values
                        .Any(eprc => eprc
                            .Any(ipc => ipc.Exposure > 0))));
            Assert.AreEqual(positiveExternalExposures.Count(), internalExposures.IndividualSubstanceTargetExposures.SelectMany(r => r.SubstanceTargetExposures).Count(c => c.SubstanceAmount > 0));
            Assert.AreEqual(100 * 24 + 1, internalExposures.IndividualSubstanceTargetExposures.SelectMany(r => r.SubstanceTargetExposures).Select(c => (SubstanceTargetExposurePattern)c).First().TargetExposuresPerTimeUnit.Count);
        }
    }
}

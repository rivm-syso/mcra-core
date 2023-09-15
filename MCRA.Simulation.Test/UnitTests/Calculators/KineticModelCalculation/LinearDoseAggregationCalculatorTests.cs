using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class LinearDoseAggregationCalculatorTests {

        /// <summary>
        ///  Linear dose aggregation: calculates individual  target exposures based on absorption factors
        /// </summary>
        [TestMethod]
        public void LinearDoseAggregationCalculatorTests_TestAbsorptionFactorModel() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var routes = new HashSet<ExposureRouteType>() { ExposureRouteType.Dermal, ExposureRouteType.Oral };
            var individuals = MockIndividualsGenerator.Create(10, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();

            var factors = new Dictionary<ExposureRouteType, double>() {
                { ExposureRouteType.Dietary, .1 },
                { ExposureRouteType.Dermal, .1 },
                { ExposureRouteType.Oral, .1 },
                { ExposureRouteType.Inhalation, .1 },
            };
            var calculator = new LinearDoseAggregationCalculator(substance, factors);
            var externalExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var result = calculator
                .CalculateIndividualDayTargetExposures(
                    externalExposures,
                    substance,
                    routes,
                    externalExposuresUnit,
                    1D,
                    new ProgressState(),
                    random
                );
            Assert.AreEqual(result.Count, individualDays.Count);
        }
    }
}

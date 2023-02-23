using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation {

    /// <summary>
    /// InternalTargetExposureCalculatorTests
    /// </summary>
    [TestClass]
    public class ExternalTargetExposuresCalculatorTests {
        /// <summary>
        /// ComputeTargetIndividualExposuresPerformanceTest
        /// </summary>
        [TestMethod]
        public void ComputeTargetIndividualDayExposuresTest() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(10);
            var individuals = MockIndividualsGenerator.Create(1000, 5, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(
                individualDays,
                substances,
                new[] { ExposureRouteType.Dietary },
                seed
            );

            //Calculate the sum of all exposures beforehand
            var sumTotalExposure = individualDayExposures
                .AsParallel()
                .Sum((System.Func<IExternalIndividualDayExposure, double>)(r => r.ExposuresPerRouteSubstance[(ExposureRouteType)ExposureRouteType.Dietary]
                           .Sum(s => s.Exposure))
            );

            var calculator = new ExternalTargetExposuresCalculator();

            var targetIndividualDayExposures = calculator
                .ComputeTargetIndividualDayExposures(
                    individualDayExposures,
                    substances,
                    null, null, null, null, null, null
                ).ToList();

            //Calculate a checksum over the substance amount property
            //using an index to multiply/divide intermediate results
            //so there is a dependency on the order of the items
            var checkSum = targetIndividualDayExposures
                .AsParallel()
                .Sum(r => r.TargetExposuresBySubstance.Values
                           .Sum(s => s.SubstanceAmount));

            Assert.AreEqual(sumTotalExposure, checkSum, 1e-3);
        }

        /// <summary>
        /// ComputeTargetIndividualExposuresPerformanceTest
        /// </summary>
        [TestMethod]
        public void ComputeTargetIndividualExposuresTest() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(10);
            var individuals = MockIndividualsGenerator.Create(1000, 5, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(
                individualDays,
                substances,
                new[] { ExposureRouteType.Dietary },
                seed
            );

            //Calculate the sum of all exposures beforehand
            var sumTotalExposure = individualDayExposures
                .AsParallel()
                .Sum(r => r.ExposuresPerRouteSubstance[ExposureRouteType.Dietary]
                           .Sum(s => s.Exposure)
            );

            var calculator = new ExternalTargetExposuresCalculator();
            var targetIndividualDayExposures = calculator
                .ComputeTargetIndividualExposures(
                    individualDayExposures,
                    substances,
                    null, null, null, null, null, null
                ).ToList();

            //Calculate a checksum over the substance amount property
            //using an index to multiply/divide intermediate results
            //so there is a dependency on the order of the items
            var checkSum = targetIndividualDayExposures
                .Sum(r => r.TargetExposuresBySubstance.Values
                           .Sum(s => s.SubstanceAmount));

            Assert.AreEqual(sumTotalExposure, checkSum, 1e-3);
        }
    }
}

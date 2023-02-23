using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, OIM
    /// </summary>
    [TestClass]
    public class ChronicNonDietarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize, test ChronicNonDietarySection view
        /// </summary>
        [TestMethod]
        public void ChronicNonDietarySection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var exposureRoutes = new List<ExposureRouteType>() { ExposureRouteType.Dietary, ExposureRouteType.Dermal, ExposureRouteType.Oral, ExposureRouteType.Inhalation };

            var foods = MockFoodsGenerator.Create(3);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var nondietaryIndividualDayIntakes = MockNonDietaryIndividualIntakeGenerator.Generate(individuals, substances, exposureRoutes, 0, random);

            var header = new SectionHeader();
            var section = new ChronicNonDietarySection();
            section.Summarize(
                header: header,
                nonDietaryIndividualMeans: nondietaryIndividualDayIntakes,
                referenceSubstance: substances.First(),
                upperPercentage: 95,
                exposureMethod: ExposureMethod.Automatic,
                selectedExposureLevels: new double[] { 005, .1, },
                selectedPercentiles: new double[] { 50, 90, 95 },
                isPerPerson: false
            );
            section.SummarizeUncertainty(
                header: header,
                nonDietaryIndividualIntakes: nondietaryIndividualDayIntakes,
                uncertaintyLowerBound: 5,
                uncertaintyUpperBound: 95
            );
        }
    }
}
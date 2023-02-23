using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock maximum concentration limits
    /// </summary>
    public static class MockMaximumConcentrationLimitsGenerator {

        /// <summary>
        /// Creates a dictionary with maximum residue limits for combinations of foods and substances
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static IDictionary<(Food Food, Compound Substance), ConcentrationLimit> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            IRandom random
        ) {
            var mrlCandidates = new double[] { 1, 0.5, 0.1, 0.05, 0.01 };
            var concentrationLimits = new Dictionary<(Food, Compound), ConcentrationLimit>();
            foreach (var food in foods) {
                foreach (var substance in substances) {
                    concentrationLimits[(food, substance)] = new ConcentrationLimit() {
                        Compound = substance,
                        Food = food,
                        EndDate = new DateTime(),
                        StartDate = new DateTime(),
                        Limit = mrlCandidates[random.Next(0, mrlCandidates.Length)],
                    };
                }
            }
            return concentrationLimits;
        }
    }
}

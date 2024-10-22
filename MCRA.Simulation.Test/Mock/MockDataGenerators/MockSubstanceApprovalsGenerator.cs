using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock substance approvals data
    /// </summary>
    public static class MockSubstanceApprovalsGenerator {

        /// <summary>
        /// Creates fake approvals for specified substances.
        /// </summary>
        public static IList<SubstanceApproval> Create(ICollection<Compound> substances) {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var result = substances.Select(s => {
                return new SubstanceApproval {
                    Substance = s,
                    IsApproved = random.Next(0, 2) != 0,
                };
            });
            return result.ToList();
        }
    }
}

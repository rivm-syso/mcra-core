using MCRA.Utils.Statistics;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock individual intake frequencies.
    /// </summary>
    public static class FakeIndividualFrequenciesGenerator {

        /// <summary>
        /// Generates individual intake frequencies based on the provided
        /// individual days.
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ICollection<IndividualFrequency> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            double minFrequency,
            double maxFrequency,
            int seed = 1
        ) {
            var rnd = new McraRandomGenerator(seed);
            var result = simulatedIndividualDays
                .GroupBy(r => r.SimulatedIndividual)
                .Select(g => {
                    var numDays = g.Count();
                    return new IndividualFrequency(g.Key) {
                        Cofactor = g.Key.Cofactor,
                        Covariable = g.Key.Covariable,
                        Nbinomial = numDays,
                        Frequency = numDays * (minFrequency + rnd.NextDouble() * (maxFrequency - minFrequency))
                    };
                })
                .ToList();
            return result;
        }
    }
}

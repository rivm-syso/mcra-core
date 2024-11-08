using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Wrappers;
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
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(r => {
                    var numDays = r.Count();
                    return new IndividualFrequency() {
                        SimulatedIndividualId = r.First().SimulatedIndividualId,
                        SamplingWeight = r.First().IndividualSamplingWeight,
                        Cofactor = r.First().Individual.Cofactor,
                        Covariable = r.First().Individual.Covariable,
                        Nbinomial = numDays,
                        Frequency = numDays * (minFrequency + rnd.NextDouble() * (maxFrequency - minFrequency))
                    };
                })
                .ToList();
            return result;
        }
    }
}

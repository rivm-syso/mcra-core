using MCRA.Utils.Statistics;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock simple individual day intakes.
    /// </summary>
    public class FakeSimpleIndividualDayIntakeGenerator {

        /// <summary>
        /// Generates individual day amounts for the specified individual days.
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="fractionZero"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static ICollection<SimpleIndividualDayIntake> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            double fractionZero,
            IRandom random
        ) {
            var mu = 1.8;
            var cvBetween = 0.5;
            var sigmaBetween = mu * cvBetween;
            var cvWithin = 0.3 * sigmaBetween;
            var sigmaSampled = 0d;
            var sampledMu = 0d;
            var idIndividuals = new List<int>();
            var result = simulatedIndividualDays
                .Select(r => {
                    if (!idIndividuals.Contains(r.SimulatedIndividual.Id)) {
                        idIndividuals.Add(r.SimulatedIndividual.Id);
                        sigmaSampled = sigmaBetween * random.NextDouble(.5, 1);
                        sampledMu = LogNormalDistribution.Draw(random, mu, sigmaSampled);
                    }
                    var draw = NormalDistribution.Draw(random, sampledMu, cvWithin * sampledMu);
                    return new SimpleIndividualDayIntake() {
                        SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                        Amount = random.NextDouble(0, 1) > fractionZero
                            ? draw
                            : 0,
                        SimulatedIndividual = r.SimulatedIndividual,
                    };
                })
                .ToList();
            return result;
        }
    }
}

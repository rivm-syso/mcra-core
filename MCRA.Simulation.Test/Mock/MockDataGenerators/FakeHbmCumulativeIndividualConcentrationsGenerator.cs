using System.Collections.Generic;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock monitoring individual concentrations
    /// </summary>
    public static class FakeHbmCumulativeIndividualConcentrationsGenerator {
        /// <summary>
        /// Creates a list of monitoring individual  concentrations
        /// </summary>
        /// <param name="simulatedIndividuals"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<HbmCumulativeIndividualConcentration> Create(
            ICollection<Individual> simulatedIndividuals, 
            IRandom random
        ) {
            var cumulativeIndividualConcentrations = new List<HbmCumulativeIndividualConcentration>();
            foreach (var item in simulatedIndividuals) {

                var result = new HbmCumulativeIndividualConcentration() {
                    Individual = item,
                    SimulatedIndividualId = item.Id,
                    CumulativeConcentration = random.NextDouble() * 100,
                };
                cumulativeIndividualConcentrations.Add(result);
            }
            return cumulativeIndividualConcentrations;
        }
    }
}

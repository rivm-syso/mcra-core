using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.IndividualDaysGenerator {
    internal static class HbmIndividualDaysGenerator {


        public static IEnumerable<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var sims = new Dictionary<int, SimulatedIndividual>();

            var individualDays = hbmSampleSubstanceCollections
                .SelectMany(r => r.HumanMonitoringSampleSubstanceRecords
                    .Select(r => (r.SimulatedIndividualId, r.Day, r.Individual))
                )
                .Distinct()
                .Select((r, ix) => {
                    if (!sims.TryGetValue(r.SimulatedIndividualId, out var sim)) {
                        sim = new(r.Individual, r.SimulatedIndividualId);
                        sims.Add(r.SimulatedIndividualId, sim);
                    }
                    var simDay = new SimulatedIndividualDay(sim) {
                        SimulatedIndividualDayId = ix,
                        Day = r.Day
                    };
                    return simDay;
                }
            );

            return individualDays;
        }
    }
}

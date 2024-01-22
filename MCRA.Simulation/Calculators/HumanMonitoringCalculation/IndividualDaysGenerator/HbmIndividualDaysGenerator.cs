using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.IndividualDaysGenerator {
    internal static class HbmIndividualDaysGenerator {
        public static IEnumerable<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var individualDays = hbmSampleSubstanceCollections
                .SelectMany(r => r.HumanMonitoringSampleSubstanceRecords
                    .Select(r => (r.SimulatedIndividualId, r.Day, r.Individual))
                )
                .Distinct()
                .Select((r, ix) => new SimulatedIndividualDay() {
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    SimulatedIndividualDayId = ix,
                    Individual = r.Individual,
                    IndividualBodyWeight = r.Individual.BodyWeight,
                    Day = r.Day
                });

            return individualDays;
        }
    }
}

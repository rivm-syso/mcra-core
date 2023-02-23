using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmCumulativeIndividualDayConcentrationCalculator {

        public List<HbmCumulativeIndividualDayConcentration> Calculate(
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var cumulativeConcentrations = hbmIndividualDayConcentrations
                .Select(c => new HbmCumulativeIndividualDayConcentration() {
                    SimulatedIndividualId = c. SimulatedIndividualId,
                    Individual = c.Individual,
                    CumulativeConcentration = activeSubstances.Sum(
                        substance => c.AverageEndpointSubstanceExposure(substance) * relativePotencyFactors[substance])

                }).ToList();
            return cumulativeConcentrations;
        }
    }
}

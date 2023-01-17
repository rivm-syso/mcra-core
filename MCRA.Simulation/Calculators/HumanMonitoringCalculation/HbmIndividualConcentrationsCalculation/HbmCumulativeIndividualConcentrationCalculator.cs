using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmCumulativeIndividualConcentrationCalculator {

        public List<HbmCumulativeIndividualConcentration> Calculate(
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var cumulativeConcentrations = hbmIndividualConcentrations
                .Select(c => new HbmCumulativeIndividualConcentration() {
                    SimulatedIndividualId = c.SimulatedIndividualId,
                    Individual = c.Individual,
                    CumulativeConcentration = activeSubstances.Sum(substance => c.ConcentrationsBySubstance[substance].Concentration * relativePotencyFactors[substance])
                }).ToList();
            return cumulativeConcentrations;
        }
    }
}

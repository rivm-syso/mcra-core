using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmCumulativeIndividualDayConcentrationCalculator {

        public List<HbmCumulativeIndividualDayCollection> Calculate(
            List<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var results = new List<HbmCumulativeIndividualDayCollection>();
            foreach (var collection in hbmIndividualDayCollections) {
                var cumulativeConcentrations = collection.HbmIndividualDayConcentrations
                    .Select(c => new HbmCumulativeIndividualDayConcentration() {
                        SimulatedIndividualId = c.SimulatedIndividualId,
                        Individual = c.Individual,
                        CumulativeConcentration = activeSubstances.Sum(
                            substance => c.AverageEndpointSubstanceExposure(substance) * relativePotencyFactors[substance])

                    }).ToList();
                var result = new HbmCumulativeIndividualDayCollection {
                    TargetUnit = collection.TargetUnit,
                    HbmCumulativeIndividualDayConcentrations = cumulativeConcentrations
                };
                results.Add(result);
            }
            return results;
        }
    }
}

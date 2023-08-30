using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmCumulativeIndividualConcentrationCalculator {

        public List<HbmCumulativeIndividualCollection> Calculate(
            List<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var results = new List<HbmCumulativeIndividualCollection>();
            foreach (var collection in hbmIndividualCollections) {
                var cumulativeConcentrations = collection.HbmIndividualConcentrations
                    .Select(c => new HbmCumulativeIndividualConcentration() {
                        SimulatedIndividualId = c.SimulatedIndividualId,
                        Individual = c.Individual,
                        CumulativeConcentration = activeSubstances.Sum(substance => c.ConcentrationsBySubstance[substance].Concentration * relativePotencyFactors[substance])
                    }).ToList();
                var result = new HbmCumulativeIndividualCollection { 
                    TargetUnit = collection.TargetUnit,
                    HbmCumulativeIndividualConcentrations = cumulativeConcentrations 
                };
                results.Add(result);
            }
            return results;
        }
    }
}

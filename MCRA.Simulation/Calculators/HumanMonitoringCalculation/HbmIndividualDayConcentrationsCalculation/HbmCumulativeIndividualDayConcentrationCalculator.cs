using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmCumulativeIndividualDayConcentrationCalculator {

        public HbmCumulativeIndividualDayCollection Calculate(
            List<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var cumulativeConcentrations = collection
                .HbmIndividualDayConcentrations
                .Select(c => new HbmCumulativeIndividualDayConcentration() {
                    Day = c.Day,
                    TimePoint = c.TimePoint,
                    SimulatedIndividual = c.SimulatedIndividual,
                    SimulatedIndividualDayId = c.SimulatedIndividualDayId,
                    CumulativeConcentration = activeSubstances.Sum(
                        substance => c.AverageEndpointSubstanceExposure(substance) * relativePotencyFactors[substance]
                    )
                })
                .ToList();

            var cumulativeCollection =  new HbmCumulativeIndividualDayCollection {
                TargetUnit = collection.TargetUnit,
                HbmCumulativeIndividualDayConcentrations = cumulativeConcentrations
            };

            return cumulativeCollection;
        }
    }
}
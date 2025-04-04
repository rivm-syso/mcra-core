﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmCumulativeIndividualConcentrationCalculator {

        public HbmCumulativeIndividualCollection Calculate(
            List<HbmIndividualCollection> hbmIndividualCollections,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var cumulativeConcentrations = collection.HbmIndividualConcentrations
                    .Select(c => new HbmCumulativeIndividualConcentration {
                        SimulatedIndividual = c.SimulatedIndividual,
                        CumulativeConcentration = activeSubstances
                            .Sum(substance => c.ConcentrationsBySubstance.TryGetValue(substance, out var r)
                                ? r.Exposure * relativePotencyFactors[substance]
                                : 0D
                            )
                    })
                    .ToList();
            return new HbmCumulativeIndividualCollection {
                TargetUnit = collection.TargetUnit,
                HbmCumulativeIndividualConcentrations = cumulativeConcentrations
            };
        }
    }
}

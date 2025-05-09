﻿using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock monitoring individual concentrations
    /// </summary>
    public static class FakeHbmCumulativeIndividualDayConcentrationsGenerator {
        /// <summary>
        /// Creates a list of monitoring individual day concentrations
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static HbmCumulativeIndividualDayCollection Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            IRandom random
        ) {
            var hbmCumulativeIndividualDayConcentrations = new List<HbmCumulativeIndividualDayConcentration>();
            foreach (var item in simulatedIndividualDays) {
                var result = new HbmCumulativeIndividualDayConcentration() {
                    Day = item.Day,
                    SimulatedIndividual = item.SimulatedIndividual,
                    CumulativeConcentration = random.NextDouble() * 100,
                };
                hbmCumulativeIndividualDayConcentrations.Add(result);
            }
            return new HbmCumulativeIndividualDayCollection {
                TargetUnit = new TargetUnit(
                        new ExposureTarget(BiologicalMatrix.Blood),
                        new ExposureUnitTriple(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Liter, TimeScaleUnit.Peak)
                    ),
                HbmCumulativeIndividualDayConcentrations = hbmCumulativeIndividualDayConcentrations
            };
        }
    }
}

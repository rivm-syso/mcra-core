using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
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
        public static List<HbmCumulativeIndividualDayCollection> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            IRandom random
        ) {
            var hbmCumulativeIndividualDayConcentrations = new List<HbmCumulativeIndividualDayConcentration>();
            foreach (var item in simulatedIndividualDays) {
                var result = new HbmCumulativeIndividualDayConcentration() {
                    Day = item.Day,
                    Individual = item.Individual,
                    SimulatedIndividualId = item.SimulatedIndividualId,
                    CumulativeConcentration = random.NextDouble() * 100,
                };
                hbmCumulativeIndividualDayConcentrations.Add(result);
            }
            return new List<HbmCumulativeIndividualDayCollection> {
                    new HbmCumulativeIndividualDayCollection{
                        TargetUnit = new TargetUnit(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Liter, TimeScaleUnit.Peak, BiologicalMatrix.Blood),
                        HbmCumulativeIndividualDayConcentrations = hbmCumulativeIndividualDayConcentrations
                }
            };
        }
    }
}

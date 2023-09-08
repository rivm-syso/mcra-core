using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock monitoring individual concentrations
    /// </summary>
    public static class FakeHbmIndividualDayConcentrationsGenerator {
        /// <summary>
        /// Creates a list of monitoring individual day concentrations
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="substances"></param>
        /// <param name="samplingMethod"></param>
        /// <param name="targetUnit"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<HbmIndividualDayCollection> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var monitoringIndividualDayConcentrations = new List<HbmIndividualDayConcentration>();
            foreach (var item in simulatedIndividualDays) {
                var result = new HbmIndividualDayConcentration() {
                    Day = item.Day,
                    Individual = item.Individual,
                    SimulatedIndividualId = item.SimulatedIndividualId,
                    SimulatedIndividualDayId = item.SimulatedIndividualDayId,
                    ConcentrationsBySubstance = substances
                        .ToDictionary(
                            c => c,
                            c => new HbmSubstanceTargetExposure() {
                                Substance = c,
                                Concentration = random.NextDouble(),
                                SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod }
                            }
                        ),
                };
                monitoringIndividualDayConcentrations.Add(result);
            }
            var hbmIndividualDayCollections = new List<HbmIndividualDayCollection>() { new HbmIndividualDayCollection() {
                        TargetUnit = targetUnit,
                        HbmIndividualDayConcentrations = monitoringIndividualDayConcentrations
                    }
                };
            return hbmIndividualDayCollections;
        }
    }
}

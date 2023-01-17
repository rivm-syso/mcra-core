using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
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
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<HbmIndividualDayConcentration> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
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
                            } as IHbmSubstanceTargetExposure
                        ),
                };
                monitoringIndividualDayConcentrations.Add(result);
            }
            return monitoringIndividualDayConcentrations;
        }
    }
}

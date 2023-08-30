using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock monitoring individual concentrations
    /// </summary>
    public static class FakeHbmIndividualConcentrationsGenerator {
        /// <summary>
        /// Creates a list of monitoring individual  concentrations
        /// </summary>
        /// <param name="simulatedIndividuals"></param>
        /// <param name="substances"></param>
        /// <param name="samplingMethod"></param>
        /// <param name="targetUnit"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<HbmIndividualCollection> Create(
            ICollection<Individual> simulatedIndividuals, 
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var monitoringIndividualConcentrations = new List<HbmIndividualConcentration>();
            foreach (var item in simulatedIndividuals) {

                var result = new HbmIndividualConcentration() {
                    Individual = item,
                    SimulatedIndividualId = item.Id,
                    ConcentrationsBySubstance = substances
                        .ToDictionary(
                            c => c,
                            c => new HbmSubstanceTargetExposure() {
                                Substance = c,
                                Concentration = random.NextDouble() * 100,
                                SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() {
                                    samplingMethod
                                },
                            } as IHbmSubstanceTargetExposure
                        ),
                };
                monitoringIndividualConcentrations.Add(result);
            }
            var hbmIndividualCollections = new List<HbmIndividualCollection>() { new HbmIndividualCollection() {
                        TargetUnit = targetUnit,
                        HbmIndividualConcentrations = monitoringIndividualConcentrations
                    }
                };
            return hbmIndividualCollections;
        }
    }
}

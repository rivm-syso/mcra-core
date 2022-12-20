using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock monitoring individual concentrations
    /// </summary>
    public static class MockMonitoringIndividualConcentrationsGenerator {
        /// <summary>
        /// Creates a list of monitoring individual  concentrations
        /// </summary>
        /// <param name="simulatedIndividuals"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<HbmIndividualConcentration> Create(
            ICollection<Individual> simulatedIndividuals, 
            ICollection<Compound> substances,
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
                            c => new HbmConcentrationByMatrixSubstance() {
                                Substance = c,
                                Concentration = random.NextDouble() * 100,
                                SamplingMethod = new HumanMonitoringSamplingMethod() {
                                    ExposureRoute = ExposureRouteType.AtTarget.ToString(),
                                    Compartment = "Blood",
                                    SampleType = "SamplingType",
                                },
                            }
                        ),
                };
                monitoringIndividualConcentrations.Add(result);
            }
            return monitoringIndividualConcentrations;
        }
    }
}

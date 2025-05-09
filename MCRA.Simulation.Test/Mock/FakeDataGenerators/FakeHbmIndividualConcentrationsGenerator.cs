﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
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
            ICollection<SimulatedIndividual> simulatedIndividuals,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            TargetUnit targetUnit,
            IRandom random
        ) {
            var monitoringIndividualConcentrations = new List<HbmIndividualConcentration>();
            foreach (var item in simulatedIndividuals) {

                var result = new HbmIndividualConcentration() {
                    SimulatedIndividual = item,
                    ConcentrationsBySubstance = substances
                        .ToDictionary(
                            c => c,
                            c => new HbmSubstanceTargetExposure() {
                                Substance = c,
                                Exposure = random.NextDouble() * 100,
                                SourceSamplingMethods = [
                                    samplingMethod
                                ],
                            }
                        ),
                };
                monitoringIndividualConcentrations.Add(result);
            }
            var hbmIndividualCollections = new List<HbmIndividualCollection>() { new() {
                        TargetUnit = targetUnit,
                        HbmIndividualConcentrations = monitoringIndividualConcentrations
                    }
                };
            return hbmIndividualCollections;
        }
    }
}

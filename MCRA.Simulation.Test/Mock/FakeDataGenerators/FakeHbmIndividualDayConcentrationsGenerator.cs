using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock monitoring individual concentrations
    /// </summary>
    public static class FakeHbmIndividualDayConcentrationsGenerator {

        /// <summary>
        /// Creates a list of monitoring individual day concentrations
        /// </summary>
        public static List<HbmIndividualDayCollection> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            List<HumanMonitoringSamplingMethod> samplingMethods,
            IRandom random
        ) {
            var targetUnits = samplingMethods.Select(s => new TargetUnit(
                new ExposureTarget(s.BiologicalMatrix),
                new ExposureUnitTriple(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Liter, TimeScaleUnit.Unspecified)
            )).ToList();

            return Create(simulatedIndividualDays, substances, samplingMethods, targetUnits, random);
        }

        /// <summary>
        /// Creates a list of monitoring individual day concentrations
        /// </summary>
        public static List<HbmIndividualDayCollection> Create(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            List<HumanMonitoringSamplingMethod> samplingMethods,
            List<TargetUnit> targetUnits,
            IRandom random
        ) {
            if (targetUnits?.Count() != samplingMethods.Count) {
                throw new ArgumentException("The number of target units should match the number of samplingMethods", $"{targetUnits}");
            }
            var hbmIndividualDayCollections = new List<HbmIndividualDayCollection>();
            for (int i = 0; i < samplingMethods.Count; i++) {
                hbmIndividualDayCollections.Add(Create(
                    simulatedIndividualDays,
                    substances,
                    samplingMethods[i],
                    targetUnits[i],
                    random
                    ));
            }

            return hbmIndividualDayCollections;
        }


        /// <summary>
        /// Creates a list of monitoring individual day concentrations
        /// </summary>
        public static HbmIndividualDayCollection Create(
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
                    SimulatedIndividualBodyWeight = item.IndividualBodyWeight,
                    ConcentrationsBySubstance = substances
                        .ToDictionary(
                            c => c,
                            c => new HbmSubstanceTargetExposure() {
                                Substance = c,
                                Exposure = random.NextDouble(),
                                SourceSamplingMethods = samplingMethod != null
                                    ? [samplingMethod]
                                    : []
                            }
                        ),
                };
                monitoringIndividualDayConcentrations.Add(result);
            }
            return new HbmIndividualDayCollection() {
                TargetUnit = targetUnit,
                HbmIndividualDayConcentrations = monitoringIndividualDayConcentrations
            };
        }
    }
}

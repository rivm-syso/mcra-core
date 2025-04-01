using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock monitoring individual concentrations
    /// </summary>
    public static class FakeHbmCumulativeIndividualConcentrationsGenerator {

        /// <summary>
        /// Creates a list of monitoring individual  concentrations
        /// </summary>
        /// <param name="simulatedIndividuals"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static HbmCumulativeIndividualCollection Create(
            ICollection<SimulatedIndividual> simulatedIndividuals,
            IRandom random
        ) {
            var cumulativeIndividualConcentrations = new List<HbmCumulativeIndividualConcentration>();
            foreach (var item in simulatedIndividuals) {

                var result = new HbmCumulativeIndividualConcentration() {
                    SimulatedIndividual = item,
                    CumulativeConcentration = random.NextDouble() * 100,
                };
                cumulativeIndividualConcentrations.Add(result);
            }

            return new HbmCumulativeIndividualCollection {
                TargetUnit = new TargetUnit(
                        new ExposureTarget(BiologicalMatrix.Blood),
                        new ExposureUnitTriple(SubstanceAmountUnit.Micrograms, ConcentrationMassUnit.Liter, TimeScaleUnit.Peak)
                    ),
                HbmCumulativeIndividualConcentrations = cumulativeIndividualConcentrations
            };
        }
    }
}

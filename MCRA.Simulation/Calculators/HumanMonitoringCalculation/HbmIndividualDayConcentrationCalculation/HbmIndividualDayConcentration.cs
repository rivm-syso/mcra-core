using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayConcentration : HbmIndividualConcentration, ITargetIndividualDayExposure {

        public string Day { get; set; }

        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The average exposure on the specified endpoint.
        /// </summary>
        /// <param name="substance"></param>
        /// <returns></returns>
        public double AverageEndpointSubstanceExposure(Compound substance) {
            return ConcentrationsBySubstance.TryGetValue(substance, out var result)
                ? result.Concentration
                : 0D;
        }
    }
}

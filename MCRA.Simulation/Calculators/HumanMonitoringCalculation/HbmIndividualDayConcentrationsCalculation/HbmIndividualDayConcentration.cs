using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayConcentration : HbmIndividualConcentration, ITargetIndividualDayExposure {

        public HbmIndividualDayConcentration() {
        }

        public HbmIndividualDayConcentration(HbmIndividualDayConcentration hbmIndividualDayConcentration) 
            : base(hbmIndividualDayConcentration) { 
        }

        public string Day { get; set; }

        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The average exposure on the specified endpoint.
        /// </summary>
        public double AverageEndpointSubstanceExposure(Compound substance) {
            return ConcentrationsBySubstance.TryGetValue(substance, out var result)
                ? result.Concentration
                : 0D;
        }

        /// <summary>
        /// Creates a clone.
        /// </summary>
        public new HbmIndividualDayConcentration Clone() {
            var clone = new HbmIndividualDayConcentration(this);
            clone.Day = Day;
            clone.SimulatedIndividualDayId = SimulatedIndividualDayId;
            return clone;
        }
    }
}

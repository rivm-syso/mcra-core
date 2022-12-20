using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayConcentration {

        /// <summary>
        /// The simulated individual id.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The simulated individual day id.
        /// </summary>
        public string SimulatedIndividualDayId {
            get {
                return $"{SimulatedIndividualId}{Day}";
            }
        }

        /// <summary>
        /// The original individual entity.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The (survey)day of the exposure.
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// The monitoring concentrations per substance.
        /// </summary>
        public Dictionary<Compound, HbmConcentrationByMatrixSubstance> ConcentrationsBySubstance { get; set; }

        /// <summary>
        /// The average exposure on the specified endpoint.
        /// </summary>
        /// <param name="substance"></param>
        /// 
        /// <returns></returns>
        public double AverageEndpointSubstanceExposure(Compound substance) {
            return ConcentrationsBySubstance.TryGetValue(substance, out var result)
                ? result.Concentration
                : 0D;
        }
    }
}

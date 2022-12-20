using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualConcentration {

        /// <summary>
        /// The simulated individual id.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The original individual entity.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The (survey)day of the exposure.
        /// </summary>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// The monitoring exposures per substance.
        /// </summary>
        public Dictionary<Compound, HbmConcentrationByMatrixSubstance> ConcentrationsBySubstance { get; set; }
    }
}

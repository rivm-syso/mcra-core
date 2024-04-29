using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class IndividualTargetExposureCollection : TargetExposure {

        /// <summary>
        /// The HBM individual day concentrations for the target.
        /// </summary>
        public ICollection<IndividualSubstanceTargetExposure> IndividualSubstanceTargetExposures { get; set; }

    }
}

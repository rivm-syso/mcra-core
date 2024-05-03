using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class TargetIndividualExposureCollection : TargetExposure {

        /// <summary>
        /// The HBM individual day concentrations for the target.
        /// </summary>
        public ICollection<TargetIndividualExposure> TargetIndividualExposures { get; set; }

    }
}

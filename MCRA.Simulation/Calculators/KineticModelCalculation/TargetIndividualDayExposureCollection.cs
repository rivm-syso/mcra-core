using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class TargetIndividualDayExposureCollection : TargetExposure {

        /// <summary>
        /// The HBM individual day concentrations for the target.
        /// </summary>
        public ICollection<TargetIndividualDayExposure> TargetIndividualDayExposures { get; set; }

    }
}

using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardExposureRatioSubstanceUncertaintyRecord  {

        /// <summary>
        /// The code of the substance.
        /// </summary>
        public string CompoundCode { get; set; }

        /// <summary>
        /// Uncertainty collection for the hazard.
        /// </summary>
        public UncertainDataPointCollection<double> Hazard { get; set; }

        /// <summary>
        /// Uncertainty collection for the exposure.
        /// </summary>
        public UncertainDataPointCollection<double> Exposure { get; set; }

        /// <summary>
        /// Uncertainty collection for the risk.
        /// </summary>
        public UncertainDataPointCollection<double> Risks { get; set; }
    }
}



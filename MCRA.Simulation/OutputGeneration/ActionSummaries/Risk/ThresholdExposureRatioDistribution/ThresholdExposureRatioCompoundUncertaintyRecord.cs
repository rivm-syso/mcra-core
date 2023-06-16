using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ThresholdExposureRatioCompoundUncertaintyRecord  {
        public string CompoundCode { get; set; }
        /// <summary>
        /// Uncertainty collection for CED
        /// </summary>
        public UncertainDataPointCollection<double> Ced { get; set; }

        /// <summary>
        /// Uncertainty collection for Exposure
        /// </summary>
        public UncertainDataPointCollection<double> Exp { get; set; }

        /// <summary>
        /// Uncertainty collection for Risk
        /// </summary>
        public UncertainDataPointCollection<double> Risks { get; set; }
    }
}



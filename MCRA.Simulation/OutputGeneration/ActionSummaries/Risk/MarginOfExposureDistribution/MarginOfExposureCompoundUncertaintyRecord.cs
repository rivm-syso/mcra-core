using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class MarginOfExposureCompoundUncertaintyRecord  {
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
        /// Uncertainty collection for IMOE
        /// </summary>
        public UncertainDataPointCollection<double> Imoe { get; set; }
    }
}



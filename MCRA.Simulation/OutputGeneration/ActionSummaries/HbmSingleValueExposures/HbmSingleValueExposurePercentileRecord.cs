using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSingleValueExposurePercentileRecord {
        [DisplayName("Percentage")]
        public string Percentage { get; set; }

        [DisplayName("Percentile")]
        public double Percentile { get; set; }
    }
}

using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureDistributionPercentileRecord : BoxPlotChartRecord {

        [DisplayName("Stratification")]
        public string Stratification { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{Stratification}";
            }
            return string.Empty;
        }
    }
}

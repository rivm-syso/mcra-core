using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourcePercentileRecord : BoxPlotChartRecord {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string ExposureSource { get; set; }

        [DisplayName("Stratification")]
        public string Stratification { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{ExposureSource} ({Stratification})";
            }
            return ExposureSource;
        }
    }
}

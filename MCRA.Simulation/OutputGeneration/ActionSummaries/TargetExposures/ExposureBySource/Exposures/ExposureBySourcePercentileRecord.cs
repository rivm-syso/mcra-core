using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourcePercentileRecord : BoxPlotChartRecord {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string ExposureSource { get; set; }

        public override string GetLabel() {
            return $"{ExposureSource}";
        }
    }
}

using System.ComponentModel;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourceSubstancePercentileRecord : BoxPlotChartRecord {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string ExposureSource { get; set; }

        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override string GetLabel() {
            return $"{ExposureSource} {SubstanceName}";
        }
    }
}

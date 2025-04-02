using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourceRouteSubstancePercentileRecord : BoxPlotChartRecord {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string ExposureSource { get; set; }

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string ExposureRoute { get; set; }
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override string GetLabel() {
            return $"{ExposureSource} {ExposureRoute} {SubstanceName}";
        }
    }
}

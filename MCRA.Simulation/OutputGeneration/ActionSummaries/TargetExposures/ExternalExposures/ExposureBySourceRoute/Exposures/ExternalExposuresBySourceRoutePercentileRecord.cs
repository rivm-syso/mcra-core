using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposuresBySourceRoutePercentileRecord : BoxPlotChartRecord {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string ExposureSource { get; set; }

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string ExposureRoute { get; set; }

        public override string GetLabel() {
            return $"{ExposureSource} {ExposureRoute}";
        }
    }
}

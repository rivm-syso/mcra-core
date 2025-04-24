using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExternalExposureByRoutePercentileRecord : BoxPlotChartRecord {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string ExposureRoute { get; set; }

        public override string GetLabel() {
            return $"{ExposureRoute}";
        }
    }
}

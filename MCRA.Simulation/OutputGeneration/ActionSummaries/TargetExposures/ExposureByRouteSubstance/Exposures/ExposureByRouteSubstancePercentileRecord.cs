using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureByRouteSubstancePercentileRecord : BoxPlotChartRecord {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string ExposureRoute { get; set; }
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override string GetLabel() {
            return $"{ExposureRoute} {SubstanceName}";
        }
    }
}

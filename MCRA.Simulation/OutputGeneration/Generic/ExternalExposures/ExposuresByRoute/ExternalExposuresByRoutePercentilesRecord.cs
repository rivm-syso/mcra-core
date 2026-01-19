using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute {
    public sealed class ExternalExposuresByRoutePercentilesRecord : BoxPlotChartRecord {

        [Description("Exposure route")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        public override string GetLabel() {
            return ExposureRoute;
       }
    }
}

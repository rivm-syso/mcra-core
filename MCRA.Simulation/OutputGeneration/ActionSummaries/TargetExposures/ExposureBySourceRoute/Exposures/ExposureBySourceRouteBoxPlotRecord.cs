using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourceRouteBoxPlotRecord : InternalExposureBoxPlotRecordBase<SourceRouteContributorKey> {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        public override string GetLabel() {
            return $"{Source} {Route}";
        }

        public override void SetDescriptorValues(SourceRouteContributorKey key) {
            Route = key.Route.GetDisplayName();
            Source = key.Source.GetDisplayName();
        }

        public override string GetKey() {
            return $"{Source} {Route}";
        }
    }
}

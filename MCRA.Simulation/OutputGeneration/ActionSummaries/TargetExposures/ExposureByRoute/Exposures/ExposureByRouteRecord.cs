using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureByRouteRecord : InternalExposureDistributionRecordBase<RouteContributorKey> {
        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        public ExposureByRouteRecord() { }

        public override void SetDescriptorValues(RouteContributorKey key) {
            Route = key.Route.GetDisplayName();
        }

        public override string GetKey() {
            return Route;
        }
    }
}

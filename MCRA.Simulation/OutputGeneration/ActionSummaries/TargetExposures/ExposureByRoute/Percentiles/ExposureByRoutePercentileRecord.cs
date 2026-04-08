using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureByRoutePercentileRecord : InternalExposurePercentileRecordBase<RouteContributorKey> {
        [Description("Specified percentage.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get { return XValue * 100; } }

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        public override void SetDescriptorValues(RouteContributorKey key) {
            Route = key.Route.GetDisplayName();
        }

        public override string GetKey() {
            return Route;
        }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureByRouteSubstancePercentileRecord : InternalExposurePercentileRecordBase<RouteSubstanceContributorKey> {
        [Description("Specified percentage.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get { return XValue * 100; } }

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override void SetDescriptorValues(RouteSubstanceContributorKey key) {
            Route = key.Route.GetDisplayName();
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Route} {Substance}";
        }
    }
}
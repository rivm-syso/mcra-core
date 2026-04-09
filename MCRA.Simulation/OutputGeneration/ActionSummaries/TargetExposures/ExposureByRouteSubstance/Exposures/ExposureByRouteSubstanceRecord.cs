using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureByRouteSubstanceRecord : InternalExposureDistributionRecordBase<RouteSubstanceContributorKey> {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override void SetDescriptorValues(RouteSubstanceContributorKey key) {
            Route = key.Route.GetDisplayName();
            SubstanceName = key.Substance.Name;
            SubstanceCode = key.Substance.Code;
        }

        public override string GetKey() {
            return $"{Route} {SubstanceCode}";
        }
    }
}
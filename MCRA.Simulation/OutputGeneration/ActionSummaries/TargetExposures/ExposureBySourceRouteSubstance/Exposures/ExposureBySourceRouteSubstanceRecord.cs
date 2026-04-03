using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceRouteSubstanceRecord : InternalExposureDistributionRecordBase<SourceRouteSubstanceContributorKey> {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        [Description("Substancee.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override void SetDescriptorValues(SourceRouteSubstanceContributorKey key) {
            Route = key.Route.GetDisplayName();
            Source = key.Source.GetDisplayName();
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Source} {Route} {Substance}";
        }
    }
}
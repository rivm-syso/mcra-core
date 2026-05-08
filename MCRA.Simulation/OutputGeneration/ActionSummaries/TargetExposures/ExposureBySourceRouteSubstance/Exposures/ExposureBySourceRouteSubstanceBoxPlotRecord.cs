using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourceRouteSubstanceBoxPlotRecord : InternalExposureBoxPlotRecordBase<SourceRouteSubstanceContributorKey> {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        public override string GetLabel() {
            return $"{Source} {Route} {SubstanceName}";
        }

        public override void SetDescriptorValues(SourceRouteSubstanceContributorKey key) {
            Route = key.Route.GetDisplayName();
            Source = key.Source.GetDisplayName();
            SubstanceName = key.Substance.Name;
        }

        public override string GetKey() {
            return $"{Source} {Route} {SubstanceName}";
        }
    }
}
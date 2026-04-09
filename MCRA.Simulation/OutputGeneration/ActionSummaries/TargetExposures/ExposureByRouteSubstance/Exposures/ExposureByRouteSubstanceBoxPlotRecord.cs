using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureByRouteSubstanceBoxPlotRecord : InternalExposureBoxPlotRecordBase<RouteSubstanceContributorKey> {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{Route} {SubstanceName} ({Stratification})";
            }
            return $"{Route} {SubstanceName}";
        }

        public override void SetDescriptorValues(RouteSubstanceContributorKey key) {
            Route = key.Route.GetDisplayName();
            SubstanceName = key.Substance.Name;
        }

        public override string GetKey() {
            return $"{Route} {SubstanceName}";
        }
    }
}
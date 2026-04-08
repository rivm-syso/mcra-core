using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureByRouteSubstanceBoxPlotRecord : InternalExposureBoxPlotRecordBase<RouteSubstanceContributorKey> {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{Route} {Substance} ({Stratification})";
            }
            return $"{Route} {Substance}";
        }

        public override void SetDescriptorValues(RouteSubstanceContributorKey key) {
            Route = key.Route.GetDisplayName();
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Route} {Substance}";
        }
    }
}
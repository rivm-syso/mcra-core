using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourceSubstanceBoxPlotRecord : InternalExposureBoxPlotRecordBase<SourceSubstanceContributorKey> {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        public override string GetLabel() {
            return $"{Source} {SubstanceName}";
        }

        public override void SetDescriptorValues(SourceSubstanceContributorKey key) {
            Source = key.Source.GetDisplayName();
            SubstanceName = key.Substance.Name;
        }

        public override string GetKey() {
            return $"{Source} {SubstanceName}";
        }
    }
}
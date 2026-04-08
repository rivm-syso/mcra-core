using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourceSubstanceBoxPlotRecord : InternalExposureBoxPlotRecordBase<SourceSubstanceContributorKey> {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{Source} {Substance} ({Stratification})";
            }
            return $"{Source} {Substance}";
        }

        public override void SetDescriptorValues(SourceSubstanceContributorKey key) {
            Source = key.Source.GetDisplayName();
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Source} {Substance}";
        }
    }
}
using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceSubstanceRecord : InternalExposureDistributionRecordBase<SourceSubstanceContributorKey> {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override void SetDescriptorValues(SourceSubstanceContributorKey key) {
            Source = key.Source.GetDisplayName();
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Source} {Substance}";
        }
    }
}
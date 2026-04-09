using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySourceSubstanceRecord : InternalExposureContributionRecordBase<SourceSubstanceContributorKey> {

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override void SetDescriptorValues(SourceSubstanceContributorKey key) {
            Source = key.Source.GetDisplayName();
            SubstanceName = key.Substance.Name;
            SubstanceCode = key.Substance.Code;
        }

        public override string GetKey() {
            return $"{Source} {SubstanceCode}";
        }
    }
}

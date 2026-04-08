using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceRecord : InternalExposureDistributionRecordBase<SourceContributorKey> {
        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        public override void SetDescriptorValues(SourceContributorKey key) {
            Source = key.Source.GetDisplayName();
        }

        public override string GetKey() {
            return Source;
        }
    }
}

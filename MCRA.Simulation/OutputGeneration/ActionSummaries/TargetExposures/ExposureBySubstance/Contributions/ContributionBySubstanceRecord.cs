using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySubstanceRecord : InternalExposureContributionRecordBase<SubstanceContributorKey> {

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override void SetDescriptorValues(SubstanceContributorKey key) {
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Substance}";
        }
    }
}


using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySubstanceRecord : InternalExposureContributionRecordBase<SubstanceContributorKey> {

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override void SetDescriptorValues(SubstanceContributorKey key) {
            SubstanceName = key.Substance.Name;
            SubstanceCode = key.Substance.Code;
        }

        public override string GetKey() {
            return $"{SubstanceCode}";
        }
    }
}


using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class SubstanceRiskDistributionRecord
        : RiskDistributionRecordBase<RiskSubstanceContributorKey> {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override void SetDescriptorValues(RiskSubstanceContributorKey key) {
            SubstanceName = key.Substance.Name;
            SubstanceCode = key.Substance.Code;
        }

        public override string GetDescriptorKey() {
            return SubstanceCode;
        }

        public override string GetLabel() {
            return SubstanceName;
        }
    }
}

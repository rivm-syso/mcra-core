using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmConcentrationBySubstanceRecord
        : HbmConcentrationDistributionRecordBase<HbmSubstanceContributorKey> {

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override void SetDescriptorValues(HbmSubstanceContributorKey key) {
            SubstanceName = key.Substance.Name;
            SubstanceCode = key.Substance.Code;
        }

        public override string GetDescriptorKey() {
            return SubstanceCode;
        }
    }
}
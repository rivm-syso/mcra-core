using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSubstancePercentilesRecord : HbmBoxPlotRecordBase<HbmSubstanceContributorKey> {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override string GetDescriptorKey() {
            return SubstanceCode;
        }

        public override string GetLabel() {
            return SubstanceName;
        }

        public override void SetDescriptorValues(HbmSubstanceContributorKey key) {
            SubstanceCode = key.Substance.Code;
            SubstanceName = key.Substance.Name;
        }
    }
}

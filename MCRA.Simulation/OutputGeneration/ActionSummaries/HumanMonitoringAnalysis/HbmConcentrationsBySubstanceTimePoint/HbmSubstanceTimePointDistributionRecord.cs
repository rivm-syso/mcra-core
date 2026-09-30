using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSubstanceTimePointDistributionRecord
        : HbmConcentrationDistributionRecordBase<HbmSubstanceTimePointContributorKey> {

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Time point code")]
        [DisplayName("Time point code")]
        public string TimePointCode { get; set; }

        [Description("Time point name")]
        [DisplayName("Time point name")]
        public string TimePointName { get; set; }

        public override void SetDescriptorValues(HbmSubstanceTimePointContributorKey key) {
            SubstanceName = key.Substance.Name;
            SubstanceCode = key.Substance.Code;
            TimePointCode = key.TimePoint.Code;
            TimePointName = key.TimePoint.Name;
        }

        public override string GetDescriptorKey() {
            return $"{SubstanceCode}-{TimePointCode}";
        }
    }
}
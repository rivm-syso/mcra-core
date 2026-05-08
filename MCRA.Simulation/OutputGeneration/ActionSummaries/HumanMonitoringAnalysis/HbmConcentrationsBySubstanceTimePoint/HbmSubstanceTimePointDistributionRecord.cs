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

        [DisplayName("Time point code")]
        public string TimePointCode { get; set; }

        [DisplayName("Time point name")]
        public string TimePointName { get; set; }

        public override void SetDescriptorValues(HbmSubstanceTimePointContributorKey key) {
            SubstanceCode = key.Substance.Code;
            SubstanceName = key.Substance.Name;
            TimePointCode = key.TimePoint.Code;
            TimePointName = key.TimePoint.Name;
        }

        public override string GetDescriptorKey() {
            return $"{SubstanceCode}-{TimePointCode}";
        }
    }
}

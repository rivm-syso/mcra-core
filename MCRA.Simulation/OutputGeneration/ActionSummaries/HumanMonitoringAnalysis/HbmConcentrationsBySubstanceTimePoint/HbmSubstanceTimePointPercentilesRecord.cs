using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmSubstanceTimePointPercentilesRecord : HbmBoxPlotRecordBase<HbmSubstanceTimePointContributorKey> {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Time point code")]
        public string TimePointCode { get; set; }

        [DisplayName("Time point name")]
        public string TimePointName { get; set; }

        public override string GetLabel() {
            return $"{SubstanceName}-{TimePointName}";
        }

        public override string GetDescriptorKey() {
            return $"{SubstanceName}-{TimePointName}";
        }

        public override void SetDescriptorValues(HbmSubstanceTimePointContributorKey key) {
            SubstanceCode = key.Substance.Code;
            SubstanceName = key.Substance.Name;
            TimePointCode = key.TimePoint.Code;
            TimePointName = key.TimePoint.Name;
        }
    }
}

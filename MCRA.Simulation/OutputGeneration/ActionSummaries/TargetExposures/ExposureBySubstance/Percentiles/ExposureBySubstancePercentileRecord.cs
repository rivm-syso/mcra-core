using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySubstancePercentileRecord : InternalExposurePercentileRecordBase<SubstanceContributorKey> {
        [Description("Specified percentage.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get { return XValue * 100; } }

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
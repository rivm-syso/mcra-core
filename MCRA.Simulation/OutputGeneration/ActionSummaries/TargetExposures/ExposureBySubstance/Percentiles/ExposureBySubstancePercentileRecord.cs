using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySubstancePercentileRecord : InternalExposurePercentileRecordBase<SubstanceContributorKey> {
        [Description("Specified percentage.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get { return XValue * 100; } }

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
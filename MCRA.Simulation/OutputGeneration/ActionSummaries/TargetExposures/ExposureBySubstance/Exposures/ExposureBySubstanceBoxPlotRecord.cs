using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySubstanceBoxPlotRecord : InternalExposureBoxPlotRecordBase<SubstanceContributorKey> {

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{Substance} ({Stratification})";
            }
            return $"{Substance}";
        }

        public override void SetDescriptorValues(SubstanceContributorKey key) {
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Substance}";
        }
    }
}
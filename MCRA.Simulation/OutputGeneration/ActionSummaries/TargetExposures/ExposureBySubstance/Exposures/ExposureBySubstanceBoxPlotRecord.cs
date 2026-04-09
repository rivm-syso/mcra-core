using System.ComponentModel;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySubstanceBoxPlotRecord : InternalExposureBoxPlotRecordBase<SubstanceContributorKey> {

        [Description("Substance name")]
        [DisplayName("Substance")]
        public string SubstanceName { get; set; }

        public override string GetLabel() {
            if (!string.IsNullOrEmpty(Stratification)) {
                return $"{SubstanceName} ({Stratification})";
            }
            return $"{SubstanceName}";
        }

        public override void SetDescriptorValues(SubstanceContributorKey key) {
            SubstanceName = key.Substance.Name;
        }

        public override string GetKey() {
            return $"{SubstanceName}";
        }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureBySourceSubstancePercentileRecord : InternalExposurePercentileRecordBase<SourceSubstanceContributorKey> {
        [Description("Specified percentage.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get { return XValue * 100; } }

        [Description("Exposure source.")]
        [DisplayName("Source")]
        public string Source { get; set; }

        [Description("Substance.")]
        [DisplayName("Substance")]
        public string Substance { get; set; }

        public override void SetDescriptorValues(SourceSubstanceContributorKey key) {
            Source = key.Source.GetDisplayName();
            Substance = key.Substance;
        }

        public override string GetKey() {
            return $"{Source} {Substance}";
        }
    }
}
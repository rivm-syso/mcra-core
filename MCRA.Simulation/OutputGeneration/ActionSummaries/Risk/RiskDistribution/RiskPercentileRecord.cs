using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class RiskPercentileRecord {
        [Display(AutoGenerateField = false)]
        public double XValues { get; set; }

        [Description("Specified percentage of the exposure distribution.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double ExposurePercentage { get; set; }

        [Description("Exposure at the specified percentile of the nominal analysis.")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double? ReferenceValueExposure { get; set; }

        [Description("Median of the uncertainty distribution of the exposure at the specified percentile.")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double? MedianExposure { get; set; }

        [Description("Lower uncertainty bound (LowerBound) of the exposure at the specified percentile.")]
        [DisplayName("Exposure (IntakeUnit) lower (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double? LowerBoundExposure { get; set; }

        [Description("Upper uncertainty bound (UpperBound) of the exposure at the specified percentile.")]
        [DisplayName("Exposure (IntakeUnit) upper (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double? UpperBoundExposure { get; set; }

        [Description("Specified percentage of the risk distribution.")]
        [DisplayName("Percentage risk distribution")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double RisksPercentage { get; set; }

        [Description("Risk ratio ({RiskMetric}) at the specified percentile of the nominal analysis.")]
        [DisplayName("Risk ratio ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double ReferenceValue { get; set; }

        [Description("Median of the uncertainty distribution of the risk ratio ({RiskMetric}) at the specified percentile.")]
        [DisplayName("Risk ratio ({RiskMetricShort}) median")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double Median { get; set; }

        [Description("Lower uncertainty bound (LowerBound) of the risk ratio ({RiskMetric}) at the specified percentile.")]
        [DisplayName("Risk ratio ({RiskMetricShort}) lower (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerBound { get; set; }

        [Description("Upper uncertainty bound (UpperBound) of the risk ratio ({RiskMetric}) at the specified percentile.")]
        [DisplayName("Risk ratio ({RiskMetricShort}) upper (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperBound { get; set; }
    }
}

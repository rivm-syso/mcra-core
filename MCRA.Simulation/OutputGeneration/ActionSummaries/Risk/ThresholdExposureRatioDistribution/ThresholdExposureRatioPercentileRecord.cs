using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class ThresholdExposureRatioPercentileRecord {
        [Display(AutoGenerateField = false)]
        public double XValues { get; set; }

        [Description("Specified percentage exposure.")]
        [DisplayName("Percentage exposure")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double ExposurePercentage => 100 - XValues * 100;

        [Description("Exposure (IntakeUnit) of the nominal analysis.")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double ReferenceValueExposure { get; set; }

        [Description("Median of the p50 of exposure uncertainty distribution.")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double MedianExposure { get; set; }

        [Description("Uncertainty lower (LowerBound) of exposure uncertainty distribution.")]
        [DisplayName("Exposure lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerBoundExposure { get; set; }

        [Description("Uncertainty upper (LowerBound) of exposure uncertainty distribution.")]
        [DisplayName("Exposure upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperBoundExposure { get; set; }

        [Description("Specified percentage threshold value/exposure.")]
        [DisplayName("Percentage Threshold/Exp")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double XValuesPercentage => XValues * 100;

        [Description("Threshold value/exposure of the nominal analysis.")]
        [DisplayName("Threshold/Exp")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double ReferenceValue { get; set; }

        [Description("Median of the p50 of the threshold value/exposure uncertainty distribution.")]
        [DisplayName("Median (p50)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double Median { get; set; }

        [Description("Uncertainty lower bound threshold value/exposure (LowerBound).")]
        [DisplayName("Lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerBound { get; set; }

        [Description("Uncertainty upper bound threshold value/exposure (UpperBound).")]
        [DisplayName("Upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperBound { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class IntakePercentageRecord {

        [Description("Exposure level (IntakeUnit).")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double XValues { get; set; }

        [Display(AutoGenerateField = false)]
        public double ReferenceValue { get; set; }

        [Description("Percentage corresponding to this exposure level.")]
        [DisplayName("Percentage (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double ReferenceValuePercentage { get { return ReferenceValue * 100; } }

        [Display(AutoGenerateField = false)]
        public double LowerBound { get; set; }

        [Description("Uncertainty lower bound (LowerBound).")]
        [DisplayName("Lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double LowerBoundPercentage { get { return LowerBound * 100; } }

        [Display(AutoGenerateField = false)]
        public double UpperBound { get; set; }

        [Description("Uncertainty upper bound (UpperBound).")]
        [DisplayName("Upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double UpperBoundPercentage { get { return UpperBound * 100; } }

        [Description("Exceedance expressed as number of people per million exceeding the exposure level.")]
        [DisplayName("Exceedance per million (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double NumberOfPeopleExceedanceExposureLevel { get; set; }

        [Description("Exceedance lower bound (LowerBound).")]
        [DisplayName("Exceedance lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double LowerBoundNumberOfPeopleExceedanceExposureLevel { get; set; }

        [Description("Exceedance upper bound (UpperBound).")]
        [DisplayName("Exceedance upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public double UpperBoundNumberOfPeopleExceedanceExposureLevel { get; set; }
    }
}

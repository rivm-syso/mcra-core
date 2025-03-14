using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class IntakePercentileRecord {
        [Display(AutoGenerateField = false)]
        public double XValues { get; set; }

        [Description("Specified percentage.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double XValuesPercentage { get { return XValues * 100; } }

        [Description("Exposure (IntakeUnit) at the specified percentage.")]
        [DisplayName("Exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double ReferenceValue { get; set; }

        [Description("Median of the p50 of the uncertainty distribution of the exposure at the specified percentage.")]
        [DisplayName("Median (p50)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double Median { get; set; }

        [Description("Lower bound (LowerBound) of the uncertainty distribution of the exposure at the specified percentage.")]
        [DisplayName("Uncertainty lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerBound { get; set; }

        [Description("Upper bound (UpperBound) of the uncertainty distribution of the exposure at the specified percentage.")]
        [DisplayName("Uncertainty upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperBound { get; set; }
    }
}

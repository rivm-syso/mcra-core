using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardPercentileRecord {
        [Display(AutoGenerateField = false)]
        public double XValues { get; set; }

        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double XValuesPercentage { get { return XValues * 100; } }

        [DisplayName("Hazard (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double ReferenceValue { get; set; }

        [DisplayName("Median (p50)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double Median { get; set; }

        [DisplayName("Lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerBound { get; set; }

        [DisplayName("Upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperBound { get; set; }
    }
}

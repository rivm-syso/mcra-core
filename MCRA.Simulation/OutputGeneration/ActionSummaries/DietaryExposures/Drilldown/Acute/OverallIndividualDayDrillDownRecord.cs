using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Overall individual drilldown.
    /// </summary>
    public sealed class OverallIndividualDayDrillDownRecord  {

        [Display(AutoGenerateField = false)]
        public int SimulatedIndividualDayId { get; set; }

        [Description("Individual ID")]
        [DisplayName("Individual ID")]
        public string IndividualId { get; set; }

        [Description("Body weight.")]
        [DisplayName("Body weight (BodyWeightUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BodyWeight { get; set; }

        [Description("Day.")]
        [DisplayName("Day")]
        public string Day { get; set; }

        [Description("Dietary exposure")]
        [DisplayName("Dietary exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double DietaryExposure { get; set; }

        [Description("Sampling weight.")]
        [DisplayName("Sampling weight")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double SamplingWeight { get; set; }
    }
}

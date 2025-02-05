using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Overall individual food drilldown.
    /// </summary>
    public sealed class IndividualFoodDrillDownRecord  {
        [Description("Day in survey.")]
        [DisplayName("Day")]
        public string Day { get; set; }

        [Description("Food name")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("Food code")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("The total consumption is the total amount of all food consumed that day.")]
        [DisplayName("Total consumption (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double TotalConsumption { get; set; }

        [Description("The net consumption is the total amount of modelled food with positive concentration values consumed that day.")]
        [DisplayName("Net consumption (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double NetConsumption { get; set; }

        [Description("Equivalent exposure.")]
        [DisplayName("Equivalent exposure (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double EquivalentExposure { get; set; }

        [Description("Cumulative exposure per food = consumption  * equivalents/ bodyWeight.")]
        [DisplayName("Exposure  (IntakeUnit) ")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

    }
}

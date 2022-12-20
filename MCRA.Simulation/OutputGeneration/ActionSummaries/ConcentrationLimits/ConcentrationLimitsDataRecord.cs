using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationLimitsDataRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [DisplayName("Start date")]
        public string StartDate { get; set; }

        [DisplayName("End date")]
        public string EndDate { get; set; }

        [Description("Concentration limit value (ConcentrationUnit).")]
        [DisplayName("Limit (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? MaximumConcentrationLimit { get; set; }

    }
}

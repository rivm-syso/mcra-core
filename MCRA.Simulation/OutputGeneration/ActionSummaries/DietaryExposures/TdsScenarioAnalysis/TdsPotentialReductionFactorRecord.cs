using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class TdsPotentialReductionFactorRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Percentile corresponding to specified percentage (ConcentrationUnit).")]
        [DisplayName("Percentile")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile { get; set; }

        [Description("Specified percentage of percentile point.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentage { get; set; }

        [Description("Limit value (ConcentrationUnit).")]
        [DisplayName("Limit (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Limit { get; set; }

        [Description("Reduction factor = limit / percentile (only displayed when Percentile > Limit)")]
        [DisplayName("Reduction factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public float ReductionFactor { get; set; }

        [Description("Food is used in scenario analysis")]
        [DisplayName("Scenario")]
        public string InScenario { get; set; }
    }
}

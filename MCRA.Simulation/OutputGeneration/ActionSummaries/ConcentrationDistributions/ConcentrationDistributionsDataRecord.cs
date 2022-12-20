using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConcentrationDistributionsDataRecord {
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("The mean concentration of the distribution.")]
        [DisplayName("Mean")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("Coefficient of variation of the concentration distribution.")]
        [DisplayName("Cv")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Cv { get; set; }

        [Description("Percentile corresponding to specified percentage.")]
        [DisplayName("Percentile")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile { get; set; }

        [Description("Specified percentage of percentile point.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentage { get; set; }

        [Description("Limit value (e.g., a legal limit value).")]
        [DisplayName("Limit")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Limit { get; set; }

        [Description("The concentration unit of the distribution.")]
        [DisplayName("Concentration unit")]
        public string Unit { get; set; }

        [Description("Reduction factor (= limit / percentile) used in reduction scenario analyses.")]
        [DisplayName("Reduction factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ReductionFactor {
            get {
                return Limit / Percentile;
            }
        }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Food/substance reduction factor record used in TDS reduction
    /// to limit scenario analyses.
    /// </summary>
    public sealed class TdsReductionFactorRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The concentration is multiplied by the reduction factor (= limit/percentile).")]
        [DisplayName("Reduction factor")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Factor { get; set; }
    }
}

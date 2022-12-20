using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MarketShareRecord {

        [Description("Brandloyalty is defined on the level of food as eaten")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }


        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Proportion marketshare")]
        [DisplayName("Proportion")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Proportion { get; set; }

        [Description("L = 0, no brandloyalty (default for unspecified foods, see table marketshares); L = 1, absolute brandloyalty")]
        [DisplayName("Proportion")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Brandloyalty { get; set; }
    }
}

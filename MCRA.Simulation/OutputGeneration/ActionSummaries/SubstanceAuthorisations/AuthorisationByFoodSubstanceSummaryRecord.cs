using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for concentrations
    /// </summary>
    public sealed class AuthorisationByFoodSubstanceSummaryRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Reference.")]
        [DisplayName("Reference")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string Reference { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UnMatchedFoodAsEatenSummaryRecord {

        [DisplayName("Food as eaten name")]
        public string FoodAsEatenName { get; set; }

        [DisplayName("Food as eaten code")]
        public string FoodAsEatenCode { get; set; }

        [DisplayName("Modelled food name")]
        public string FoodAsMeasuredName { get; set; }

        [DisplayName("Modelled food code")]
        public string FoodAsMeasuredCode { get; set; }

        [Display(AutoGenerateField = true)]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = true)]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

    }
}

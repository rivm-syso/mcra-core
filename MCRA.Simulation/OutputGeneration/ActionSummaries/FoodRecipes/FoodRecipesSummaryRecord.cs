using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodRecipesSummaryRecord {

        [DisplayName("As eaten name")]
        public string AsEatenRecipeName { get; set; }

        [DisplayName("As eaten code")]
        public string AsEatenRecipeCode { get; set; }

        [DisplayName("Converted to name")]
        public string ConvertedRecipeName { get; set; }

        [DisplayName("Converted to code")]
        public string ConvertedRecipeCode { get; set; }

        [DisplayName("Proportion")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Proportion { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> IntermediateCodes { get; set; }

        [DisplayName("Intermediate steps count")]
        public int IntermediateStepsCount {
            get {
                return IntermediateCodes.Count;
            }
        }

        [DisplayName("Intermediate steps")]
        public string IntermediateSteps {
            get {
                return string.Join(" > ", IntermediateCodes);
            }
        }
    }
}

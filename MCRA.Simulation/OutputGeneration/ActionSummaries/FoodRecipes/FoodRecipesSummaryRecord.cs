using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodRecipesSummaryRecord {

        [DisplayName("As eaten name")]
        [Description("Name of the consumed food (as-eaten).")]
        public string AsEatenRecipeName { get; set; }

        [DisplayName("As eaten code")]
        [Description("Code of the consumed food (as-eaten).")]
        public string AsEatenRecipeCode { get; set; }

        [DisplayName("Converted to name")]
        [Description("Name of the converted food (as-measured).")]
        public string ConvertedRecipeName { get; set; }

        [DisplayName("Converted to code")]
        [Description("Code of the converted food (as-measured).")]
        public string ConvertedRecipeCode { get; set; }

        [DisplayName("Proportion")]
        [Description("Proportion of the converted food in the as-eaten food (as a fraction).")]
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

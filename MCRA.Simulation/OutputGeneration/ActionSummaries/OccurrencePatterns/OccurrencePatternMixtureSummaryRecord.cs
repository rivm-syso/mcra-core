using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for concentrations
    /// </summary>
    public sealed class OccurrencePatternMixtureSummaryRecord {

        [Description("The food name.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("The food code.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> SubstanceNames { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> SubstanceCodes { get; set; }

        [Description("The codes of the substances in the pattern/mixture.")]
        [DisplayName("Substance codes")]
        public string CompoundCodesString {
            get {
                return string.Join(", ", SubstanceCodes);
            }
        }

        [Description("The names of the substances in the pattern/mixture.")]
        [DisplayName("Substance names")]
        public string CompoundNamesString {
            get {
                return string.Join(", ", SubstanceNames);
            }
        }

        [Description("The occurrence/use frequency of the pattern/mixture.")]
        [DisplayName("Occurrence frequency")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double AgriculturalUseFraction { get; set; }

        [Description("Is this use pattern assumed to originate from authorised use.")]
        [DisplayName("Authorised use")]
        public bool? FromAuthorisedUse { get; set; }

        [Description("The number of substances in the pattern/mixture.")]
        [DisplayName("Number of substances")]
        public int NumberOfSubstances {
            get {
                return SubstanceCodes.Count;
            }
        }
    }
}

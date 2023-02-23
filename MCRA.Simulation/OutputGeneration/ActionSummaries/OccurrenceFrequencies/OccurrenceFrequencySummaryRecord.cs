using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for concentrations
    /// </summary>
    public sealed class AgriculturalUseByFoodSubstanceSummaryRecord {

        [Description("The food name.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("The food code.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("The substance name.")]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("The substance code.")]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Location(s) to which the potential presence applies.")]
        [DisplayName("Location(s)")]
        public string Location { get; set; }

        [Description("Specifies whether substance use is authorised for this food.")]
        [DisplayName("Authorised")]
        public bool? IsAuthorised { get; set; }

        [Description("The potential presence percentage of the substance in a food sample of the given origin.")]
        [DisplayName("Potential presence")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double AgriculturalUseFraction { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AgriculturalUseFractionUncertaintyValues { get; set; }

        [Description("Potential presence lower bound (2.5 percentile).")]
        [DisplayName("Potential presence lower bound (2.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double AgriculturalUseFractionLowerBoundPercentile {
            get {
                if (AgriculturalUseFractionUncertaintyValues.Count > 1) {
                    return AgriculturalUseFractionUncertaintyValues.Percentile(2.5);
                }
                return double.NaN;
            }
        }

        [Description("Potential presence upper bound (97.5 percentile).")]
        [DisplayName("Potential presence upper bound (97.5 percentile)")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double AgriculturalUseFractionUpperBoundPercentile {
            get {
                if (AgriculturalUseFractionUncertaintyValues.Count > 1) {
                    return AgriculturalUseFractionUncertaintyValues.Percentile(97.5);
                }
                return double.NaN;
            }
        }

        [Description("Potential presence uncertainty mean value.")]
        [DisplayName("Potential presence uncertainty mean value")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public double AgriculturalUseFractionMeanUncertaintyValue {
            get {
                if (AgriculturalUseFractionUncertaintyValues.Count > 1) {
                    return AgriculturalUseFractionUncertaintyValues.Average();
                }
                return double.NaN;
            }
        }
    }
}

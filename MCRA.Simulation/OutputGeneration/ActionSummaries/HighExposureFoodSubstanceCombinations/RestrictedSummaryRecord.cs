using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RestrictedSummaryRecord {

        [Display(AutoGenerateField = false)]
        public string CodeMSCC {
            get {
                return CompoundCode + FoodAsMeasuredCode;
            }
        }

        [DisplayName("Compound")]
        [Description("A compound, food as eaten and modelled foodcombine to a source-substance-combination (SCC or risk driver component). All remaining categories are indicated as 'others'.")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [DisplayName("Modelled food")]
        [Description("A compound, a food as eaten and a modelled food combine to a source-substance-combination (SCC or risk driver component). All remaining categories are indicated as 'others'.")]
        public string FoodAsMeasuredName { get; set; }

        [DisplayName("Modelled food code")]
        public string FoodAsMeasuredCode { get; set; }

        [DisplayName("Number of foods as eaten")]
        [Description("The number of foods as eaten for a modelled-food substance combination (MFCC or risk drivers)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfFoods { get; set; }

        [DisplayName("Importance (%)")]
        [Description("Contribution to the high exposures")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Contribution { get; set; }

        [DisplayName("Cumulative importance (%)")]
        [Description("Cumulative contribution to high exposures")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double CumulativeContributionFraction { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ScreeningSummaryRecord {

        [Display(AutoGenerateField = false)]
        public string CodeSCC {
            get {
                return CompoundCode + FoodAsMeasuredCode + FoodAsEatenCode;
            }
        }

        [Display(AutoGenerateField = false)]
        public string CodeMSCC {
            get {
                return CompoundCode + FoodAsMeasuredCode;
            }
        }

        [Description("A compound, food as eaten and modelled food combine to a source-substance-combination (SCC or risk driver component)")]
        [DisplayName("Compound")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("A compound, food as eaten and modelled food combine to a source-substance-combination (SCC or risk driver component)")]
        [DisplayName("Food as eaten")]
        public string FoodAsEatenName { get; set; }

        [DisplayName("Food as eaten code")]
        public string FoodAsEatenCode { get; set; }

        [Description("A compound, food as eaten and modelled food combine to a source-substance-combination (SCC or risk driver component)")]
        [DisplayName("Modelled food")]
        public string FoodAsMeasuredName { get; set; }

        [DisplayName("Modelled food code")]
        public string FoodAsMeasuredCode { get; set; }

        [Description("Cumulative upper probability (%) to exceed the limit (= highest selected percentile among risk driver components")]
        [DisplayName("Probability above limit (%)")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Cup { get; set; }

        [Description("Contribution (%) to the sum of all exposures above the limit (importance of risk driver component)")]
        [DisplayName("Importance (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double CupPercentage { get; set; }

        [Description("Cumulative importance of ordered risk driver components (%)")]
        [DisplayName("Cumulative importance (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double CumCupPercentage { get; set; }

        [Description("Estimated exposure at the specified percentage (e.g. 99%)")]
        [DisplayName("Exposure percentile (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("Scaled density of the censored values exposure distributions at specified percentile (e.g. p99)")]
        [DisplayName("Weight censored values at percentile (%)")]
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public double WeightCensoredValues { get; set; }

        [Description("Scaled density of the positives exposure distributions at specified percentile (e.g. p99)")]
        [DisplayName("Weight detects at percentile (%)")]
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public double WeightDetect { get; set; }

        [Description("Parameter p of the mixture component distribution describing exposure due to censored value concentrations")]
        [DisplayName("Fraction exposure due to censored values")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double FractionCensoredValues { get; set; }

        [Description("Parameter mu of the mixture component distribution describing exposure due to censored value concentrations")]
        [DisplayName("Mean ln(exposure) censored values (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double MuCensoredValues { get; set; }

        [Description("Parameter sigma of the mixture component distribution describing exposure due to censored value concentrations")]
        [DisplayName("SD ln(exposure) censored values")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double SigmaCensoredValues { get; set; }

        [Description("Parameter p of the mixture component distribution describing exposure due to positive concentrations")]
        [DisplayName("Fraction exposure due to positives")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double FractionDetects { get; set; }

        [Description("Parameter mu of the mixture component distribution describing exposure due to positive concentrations")]
        [DisplayName("Mean ln(exposure) detects (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double MuDetects { get; set; }

        [Description("Parameter sigma of the mixture component distribution describing exposure due to positive concentrations")]
        [DisplayName("SD ln(exposure) detects")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double SigmaDetects { get; set; }

    }
}

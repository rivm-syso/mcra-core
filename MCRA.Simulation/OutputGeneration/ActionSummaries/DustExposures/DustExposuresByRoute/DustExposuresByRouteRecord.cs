using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DustExposuresByRouteRecord {

        [Display(AutoGenerateField = false)]
        public double LowerUncertaintyBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UpperUncertaintyBound { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }
        
        [Description("The exposure route of the exposure estimates derived from dust data.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("The exposure unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Mean exposure value for all individuals.")]
        [DisplayName("Mean all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("Median exposure value for all individuals.")]
        [DisplayName("Median all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> MedianAllUncertaintyValues { get; set; }

        [Description("Median (p50) of median of measurement values of all individuals (corrected for specific gravity correction factor).")]
        [DisplayName("Median all individuals Unc (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAllMedianPercentile {
            get {
                if (MedianAllUncertaintyValues?.Count > 0) {
                    return MedianAllUncertaintyValues.Percentile(50);
                }
                return double.NaN;
            }
        }

        [Description("Uncertainty bound (LowerBound) of median of measurement values of all individuals (corrected for specific gravity correction factor).")]
        [DisplayName("Median all individuals Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAllLowerBoundPercentile {
            get {
                if (MedianAllUncertaintyValues?.Count > 0) {
                    return MedianAllUncertaintyValues.Percentile(LowerUncertaintyBound);
                }
                return double.NaN;
            }
        }

        [Description("Uncertainty bound (UpperBound) of median of measurement values of all individuals (corrected for specific gravity correction factor).")]
        [DisplayName("Median all individuals Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAllUpperBoundPercentile {
            get {
                if (MedianAllUncertaintyValues?.Count > 0) {
                    return MedianAllUncertaintyValues.Percentile(UpperUncertaintyBound);
                }
                return double.NaN;
            }
        }

        [Description("Lower percentile point of the exposures for all individuals.")]
        [DisplayName("{LowerPercentage} all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentileAll { get; set; }

        [Description("Upper percentile point of the exposures for all individuals.")]
        [DisplayName("{UpperPercentage} all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentileAll { get; set; }

        [Description("Average of exposure values of the individuals with exposure values > 0.")]
        [DisplayName("Mean individuals positive exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("Median of exposure values of the individuals with exposure values > 0.")]
        [DisplayName("Median individuals positive exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Lower percentile point of exposure values of the individuals with exposure values > 0.")]
        [DisplayName("{LowerPercentage} individuals positive exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("Upper percentile point of exposure values of the individuals with exposure values > 0.")]
        [DisplayName("{UpperPercentage} individuals positive exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }

        [Description("Percentage of individuals with exposure value > 0.")]
        [DisplayName("Percentage individuals positive exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get; set; }
    }
}

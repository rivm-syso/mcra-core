using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRouteSubstance {

    public class ExternalExposuresByRouteSubstanceRecord {

        [Display(AutoGenerateField = false)]
        public double LowerUncertaintyBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UpperUncertaintyBound { get; set; }

        [Description("The exposure route of the exposure estimates derived from the data.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Display(AutoGenerateField = false)]
        [Description("The exposure unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Number of individuals with exposure greater than zero.")]
        [DisplayName("Individuals with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int IndividualsWithPositiveExposure { get; set; }

        [Description("Mean exposure for a route-substance for all individuals.")]
        [DisplayName("Mean all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("p50 percentile of all exposure values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median all individuals (ExposureUnit)")]
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

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentileAll { get; set; }

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentileAll { get; set; }

        [Description("Percentage of individuals with exposure greater than zero.")]
        [DisplayName("Percentage individuals with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get; set; }

        [Description("Mean exposure for individuals with exposures greater than zero (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Mean individuals exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("p50 percentile for individuals for exposures greater than zero (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median individuals exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianPositives { get; set; }

        [Description("Percentile point for individuals for exposures greater than zero (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} individuals exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("Percentile point for individuals for exposures greater than zero (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} individuals exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }
    }
}

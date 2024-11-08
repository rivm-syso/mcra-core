using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmIndividualDistributionBySubstanceRecord {

        [Display(AutoGenerateField = false)]
        public string CodeTargetSurface { get; set; }

        [Display(AutoGenerateField = false)]
        public double LowerUncertaintyBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UpperUncertaintyBound { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Sampling method(s) of the measurement(s) from which the concentration value is derived (within parenthesis the number of samples).")]
        [DisplayName("Source sampling method")]
        public string SourceSamplingMethods { get; set; }

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("The exposure route of the external exposure estimates derived from HBM data.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("The target unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("The way in which the concentration values are standardised, normalised, or otherwise expressed.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Description("Mean measurement value of all individuals (corrected for specific gravity correction factor).")]
        [DisplayName("Mean all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("Median of measurement values of all individuals (corrected for specific gravity correction factor).")]
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

        [Description("Lower percentile point of measurement values of all individuals (corrected for specific gravity correction factor).")]
        [DisplayName("{LowerPercentage} all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentileAll { get; set; }

        [Description("Upper percentile point of measurement values of all individuals (corrected for specific gravity correction factor).")]
        [DisplayName("{UpperPercentage} all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentileAll { get; set; }

        [Description("Average of measurement values of the individuals with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("Mean individuals positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("Median of measurement values of the individuals with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("Median individuals positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianPositives { get; set; }

        [Description("Lower percentile point of measurement values of the individuals with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("{LowerPercentage} individuals positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("Upper percentile point of measurement values of the individuals with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("{UpperPercentage} individuals positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }

        [Description("Number of individuals with concentrations > 0.")]
        [DisplayName("Individuals with positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int IndividualsWithPositiveConcentrations { get; set; }

        [Description("Percentage of individuals with concentrations value > 0.")]
        [DisplayName("Percentage individuals with positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get; set; }
    }
}

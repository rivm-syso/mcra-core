using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalScenarioExposureDistributionRecord {

        [Display(AutoGenerateField = false)]
        public double LowerUncertaintyBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UpperUncertaintyBound { get; set; }

        [DisplayName("Scenario code")]
        public string ScenarioCode { get; set; }

        [DisplayName("Scenario name")]
        public string ScenarioName { get; set; }

        [Description("The exposure route.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("The exposure unit.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("The estimate type.")]
        [DisplayName("Estimate type")]
        public string EstimateType { get; set; }

        [Description("Mean exposure.")]
        [DisplayName("Mean")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("Median (p50) percentile of the exposure distribution.")]
        [DisplayName("Median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> MedianUncertaintyValues { get; set; }

        [Description("Uncertainty median of the median of the exposure distribution.")]
        [DisplayName("Median - Unc (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianUncMedianPercentile {
            get {
                if (MedianUncertaintyValues?.Count > 0) {
                    return MedianUncertaintyValues.Percentile(50);
                }
                return double.NaN;
            }
        }

        [Description("Uncertainty bound (LowerBound) of the median of the exposure distribution.")]
        [DisplayName("Median - Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianUncLowerPercentile {
            get {
                if (MedianUncertaintyValues?.Count > 0) {
                    return MedianUncertaintyValues.Percentile(LowerUncertaintyBound);
                }
                return double.NaN;
            }
        }

        [Description("Uncertainty bound (UpperBound) of the median of the exposure distribution.")]
        [DisplayName("Median - Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianUncUpperPercentile {
            get {
                if (MedianUncertaintyValues?.Count > 0) {
                    return MedianUncertaintyValues.Percentile(UpperUncertaintyBound);
                }
                return double.NaN;
            }
        }

        [Description("Lower {LowerPercentage} percentile point of the exposure distribution.")]
        [DisplayName("Lower {LowerPercentage}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentile { get; set; }

        [Description("Upper {UpperPercentage} percentile point of the exposure distribution.")]
        [DisplayName("Upper {UpperPercentage}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentile { get; set; }

    }
}
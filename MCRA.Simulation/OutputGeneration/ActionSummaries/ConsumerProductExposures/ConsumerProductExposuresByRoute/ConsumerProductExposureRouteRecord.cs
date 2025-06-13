using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for consumer product routes, relative contribution to the upper tail of the exposure distribution.
    /// </summary>
    public sealed class ConsumerProductExposureRouteRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Route")]
        public string ExposureRoute { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of a route to exposure.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a route to exposure.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a route to exposure.")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of a route to exposure.")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of individuals for chronic (not corrected for sampling weights).")]
        [DisplayName("Individuals with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NumberOfIndividualDays { get; set; }

        [Description("Mean exposure for a route on all individuals (chronic).")]
        [DisplayName("Mean exposure for all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Total { get; set; }

        [Description("p50 percentile of all exposure values.")]
        [DisplayName("Median for all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point of all exposure values (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} for all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point of all exposure values (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} for all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Display(AutoGenerateField = false)]
        public double FractionPositive { get; set; }

        [Description("Percentage of individuals (chronic) with exposure (if applicable weighted with sampling weights).")]
        [DisplayName("Percentage individuals with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get { return FractionPositive * 100; } }

        [Description("Average, for exposures > 0.")]
        [DisplayName("Mean for individuals exposure > 0 (ExposureUnit).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("p50 percentile for exposures > 0 ")]
        [DisplayName("Median for individuals exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point for exposures > 0 (default 25%, see Output settings) of exposure distribution.")]
        [DisplayName("{LowerPercentage} for individuals  exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point for exposures > 0 (default 75%, see Output settings) of exposure distribution.")]
        [DisplayName("{UpperPercentage} for individuals  exposure > 0 (ExposureUnit))")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }

        [Description("Number of substances.")]
        [DisplayName("Number of substances included")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NumberOfSubstances { get; set; }
    }
}

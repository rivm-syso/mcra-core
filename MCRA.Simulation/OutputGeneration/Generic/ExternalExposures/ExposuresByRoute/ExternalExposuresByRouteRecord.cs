using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute {

    public class ExternalExposuresByRouteRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Route")]
        public string ExposureRoute { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of a route to the total exposure.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a route to exposure.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Display(AutoGenerateField = false)]
        [Description("The exposure unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Lower uncertainty bound relative contribution of a route to exposure.")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of a route to exposure.")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of individuals with exposure greater than zero.")]
        [DisplayName("Individuals with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? IndividualsWithPositiveExposure { get; set; }

        [Description("Mean exposure for a route for all individuals.")]
        [DisplayName("Mean all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("p50 percentile of all exposure values.")]
        [DisplayName("Median all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point of all exposure values (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point of all exposure values (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} all individuals (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Display(AutoGenerateField = false)]
        public double FractionPositive { get; set; }

        [Description("Percentage of individuals with exposure greater than zero (if applicable weighted with sampling weights).")]
        [DisplayName("Percentage individuals with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get { return FractionPositive * 100; } }

        [Description("Mean for individuals with exposures greater than zero.")]
        [DisplayName("Mean individuals exposure > 0 (ExposureUnit).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("Median (p50) percentile for individuals with exposures greater than zero.")]
        [DisplayName("Median individuals exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianPositives { get; set; }

        [Description("Percentile point for individuals with exposures greater than zero (default 25%, see Output settings) of exposure distribution.")]
        [DisplayName("{LowerPercentage} individuals exposure > 0 (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point for individuals with exposures greater than zero (default 75%, see Output settings) of exposure distribution.")]
        [DisplayName("{UpperPercentage} individuals exposure > 0 (ExposureUnit))")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }

        [Description("Number of substances.")]
        [DisplayName("Number of substances included")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NumberOfSubstances { get; set; }
    }
}

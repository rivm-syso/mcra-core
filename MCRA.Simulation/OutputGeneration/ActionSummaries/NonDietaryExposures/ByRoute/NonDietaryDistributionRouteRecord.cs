using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryDistributionRouteRecord {
        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [Description("non dietary exposure route")]
        [DisplayName("Exposure Route")]
        public string ExposureRoute { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of an exposure route to the nondietary exposure, including RPF's and membership probabilities")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a substance and route to exposure.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of an exposure route to exposure.")]
        [DisplayName("Lower bound (%) (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of an exposure route to exposure.")]
        [DisplayName("Upper bound (%) (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfDays { get; set; }

        [Description("Mean exposure for a route on all individual days (acute) or individuals (chronic)")]
        [DisplayName("Mean exposure all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Total { get { return Mean * Percentage / 100; } }

        [Description("p50 percentile for all exposures of nondietary exposure route values")]
        [DisplayName("Median all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point for all exposures of nondietary exposure route values (default 25%, see Output settings) ")]
        [DisplayName("{LowerPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("percentile point for all exposures of nondietary exposure route values (default 75%, see Output settings)")]
        [DisplayName("{UpperPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) with exposure")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Average for exposures > 0 of nondietary exposure route values")]
        [DisplayName("Mean {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("p50 percentile for exposures > 0 of nondietary exposure route values")]
        [DisplayName("Median {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point for exposures > 0 of nondietary exposure route values (default 25%, see Output settings) ")]
        [DisplayName("{LowerPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point for exposures > 0 of nondietary exposure route values (default 75%, see Output settings)")]
        [DisplayName("{UpperPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }
    }
}

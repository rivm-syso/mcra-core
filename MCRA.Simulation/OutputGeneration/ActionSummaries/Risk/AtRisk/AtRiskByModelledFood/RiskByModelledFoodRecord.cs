using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Risk by modelled food record.
    /// </summary>
    public sealed class RiskByModelledFoodRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Food name")]
        [Description("Name of the modelled food.")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        [Description("Code of the modelled food.")]
        public string FoodCode { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of the food to the risk.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of the food to the risk ({RiskMetric}).")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound of the relative contribution of the food to the risk ({RiskMetric}).")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound of the relative contribution of the food to the risk ({RiskMetric}).")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of {IndividualDayUnit} with intake/exposure.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? PositivesCount { get; set; }

        [Description("Mean risk ({RiskMetric}) of the food of all {IndividualDayUnit}.")]
        [DisplayName("Mean for all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("Median (p50) percentile point of the risk ({RiskMetric}) of the food of all {IndividualDayUnit}.")]
        [DisplayName("Median all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Lower ({LowerPercentage}) percentile point of risk ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("{LowerPercentage} all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Upper ({UpperPercentage}) percentile point of risk ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("{UpperPercentage} all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Display(AutoGenerateField = false)]
        public double FractionPositives { get; set; }

        [Description("Percentage of {IndividualDayUnit} with exposure.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get { return FractionPositives * 100; } }

        [Description("Average risk ({RiskMetric}) for {IndividualDayUnit} with exposures > 0.")]
        [DisplayName("Mean {IndividualDayUnit} exposure > 0 ({RiskMetricShort}).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("Median (p50) percentile point of the risk ({RiskMetric}) for {IndividualDayUnit} with exposures > 0.")]
        [DisplayName("Median {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Lower ({LowerPercentage}) percentile point of the risk ({RiskMetric}) for {IndividualDayUnit} with exposures > 0.")]
        [DisplayName("{LowerPercentage} {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Upper ({UpperPercentage}) percentile point of the risk ({RiskMetric}) for {IndividualDayUnit} with exposures > 0.")]
        [DisplayName("{UpperPercentage} {IndividualDayUnit} exposure > 0 ({RiskMetricShort}))")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }

        [Description("Number of substances included.")]
        [DisplayName("Number of substances included")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NumberOfSubstances { get; set; }
    }
}

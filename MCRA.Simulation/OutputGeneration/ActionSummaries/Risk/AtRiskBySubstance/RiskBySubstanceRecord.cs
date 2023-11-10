using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for substances, relative contribution to the risk distribution.
    /// </summary>
    public sealed class RiskBySubstanceRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Substance name")]
        [Description("Name of the substance.")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        [Description("Code of the substance.")]
        public string SubstanceCode { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of {IndividualDayUnit} with exposure.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PositivesCount { get; set; }

        [Description("Mean risk ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("Mean for all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("Median (p50) percentile point of the risk ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("Median for all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Lower ({LowerPercentage}) percentile point of the risk ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("{LowerPercentage} for all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Upper ({UpperPercentage}) percentile point of the risk ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("{UpperPercentage} for all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Display(AutoGenerateField = false)]
        public double FractionPositives { get; set; }

        [Description("Percentage of {IndividualDayUnit} with exposure.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get { return FractionPositives * 100; } }

        [Description("Average risk ({RiskMetric}) of the {IndividualDayUnit} with exposure > 0.")]
        [DisplayName("Mean for {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("Median (p50) percentile point of the risk ({RiskMetric}) of the {IndividualDayUnit} with exposure > 0.")]
        [DisplayName("Median for {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Lower ({LowerPercentage}) percentile point of the risk ({RiskMetric}) of the {IndividualDayUnit} with exposure > 0.")]
        [DisplayName("{LowerPercentage} for {IndividualDayUnit} exposure > 0 ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Upper ({UpperPercentage}) percentile point of the risk ({RiskMetric}) of the {IndividualDayUnit} with exposure > 0.")]
        [DisplayName("{UpperPercentage} for {IndividualDayUnit} exposure > 0 ({RiskMetricShort}))")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }
    }
}

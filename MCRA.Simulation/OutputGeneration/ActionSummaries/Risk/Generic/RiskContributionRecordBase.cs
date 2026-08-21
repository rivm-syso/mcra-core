using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class RiskContributionRecordBase<T>
        where T : IExposureContributorKey, new() {

        [Display(AutoGenerateField = false)]
        public int UncertaintyCycles { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> StratificationSubLevelCodes { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> StratificationSubLevelNames { get; set; }

        [DisplayName("Stratification")]
        public string Stratification { get; set; }

        [Display(AutoGenerateField = false)]
        public string StratificationLevelCode { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [Description("Code of the biological matrix.")]
        [DisplayName("Biological matrix code")]
        public string BiologicalMatrix{ get; set; }

        [Description("Relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Contribution { get; set; }

        [Description("Mean relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Sum() / UncertaintyCycles : 0D; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.PercentilesAdditionalZeros([UncertaintyLowerBound], UncertaintyCycles - Contributions.Count)[0]; } }

        [Description("Upper uncertainty bound relative contribution of a substance to the risk.")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.PercentilesAdditionalZeros([UncertaintyUpperBound], UncertaintyCycles - Contributions.Count)[0]; } }

        [Description("Number of {IndividualDayUnit} with exposure.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PositivesCount { get; set; }

        [Description("Mean risk ({RiskMetric}) of all {IndividualDayUnit}.")]
        [DisplayName("Mean for all {IndividualDayUnit} ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Display(AutoGenerateField = false)]
        public double FractionPositives { get; set; }

        [Description("Percentage of {IndividualDayUnit} with exposure.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get { return FractionPositives * 100; } }

        public abstract string GetDescriptorKey();

        public abstract void SetDescriptorValues(T descriptor);
    }
}
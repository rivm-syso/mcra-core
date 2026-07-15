using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class HbmContributionRecordBase<T>
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

        [Description("Relative contribution of a substance to risk (RPF weighted)")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Contribution { get; set; }

        [Description("Mean relative contribution.")]
        [DisplayName("Contribution mean (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Sum() / UncertaintyCycles : 0D; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution.")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.PercentilesAdditionalZeros([UncertaintyLowerBound], UncertaintyCycles - Contributions.Count)[0]; } }

        [Description("Upper uncertainty bound relative contribution.")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.PercentilesAdditionalZeros([UncertaintyUpperBound], UncertaintyCycles - Contributions.Count)[0]; } }

        [DisplayName("Number of {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfConcentrations { get; set; }

        public abstract string GetDescriptorKey();

        public abstract void SetDescriptorValues(T descriptor);
    }
}

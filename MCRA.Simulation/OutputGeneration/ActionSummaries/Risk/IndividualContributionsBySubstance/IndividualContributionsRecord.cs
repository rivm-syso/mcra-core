using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MathNet.Numerics.Statistics;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk {
    public sealed class IndividualContributionsRecord {
        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }
        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Code of the biological matrix.")]
        [DisplayName("Biological matrix code")]
        public string BiologicalMatrix { get; set; }

        [Description("Expression type.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Mean contribution (%) for individuals to a substance.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Contribution { get; set; }

        [Description("Mean contribution (%) for individuals to a substance.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Description("Lower uncertainty bound contribution (%).")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound contribution (%).")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

    }
}

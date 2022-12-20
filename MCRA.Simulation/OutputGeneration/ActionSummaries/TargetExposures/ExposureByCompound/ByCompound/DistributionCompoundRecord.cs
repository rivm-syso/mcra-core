using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for substances, relative contribution to the upper exposure distribution.
    /// </summary>
    public sealed class DistributionCompoundRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of a substance to exposure")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a substance to exposure.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a substance to exposure.")]
        [DisplayName("Lower bound (%) (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of a substance to exposure.")]
        [DisplayName("Upper bound (%) (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int N { get; set; }

        [Description("Mean exposure for a substance on all individual days (acute) or individuals (chronic)")]
        [DisplayName("Mean exposure for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("p50 percentile of all exposure values (expressed per substance [not in equivalents of reference substance])")]
        [DisplayName("Median for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings) ")]
        [DisplayName("{LowerPercentage} for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings) ")]
        [DisplayName("{UpperPercentage} for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) with exposure")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Average exposure value, for exposures > 0 (expressed per substance [not in equivalents of reference substance])")]
        [DisplayName("Mean for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Total { get { return Mean * 100 / Percentage; } }

        [Description("p50 percentile, for exposures > 0 (expressed per substance [not in equivalents of reference substance])")]
        [DisplayName("Median for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point, for exposures > 0 of exposure values (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings) ")]
        [DisplayName("{LowerPercentage} for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point, for exposures > 0 of exposure values (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings) ")]
        [DisplayName("{UpperPercentage} for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }

        [Description("Relative potency factor. RPFs are not applied except for the exposure contribution.")]
        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double RelativePotencyFactor { get; set; }

        [Description("Assessment group membership.")]
        [DisplayName("P(AG)")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double AssessmentGroupMembership { get; set; }

    }
}

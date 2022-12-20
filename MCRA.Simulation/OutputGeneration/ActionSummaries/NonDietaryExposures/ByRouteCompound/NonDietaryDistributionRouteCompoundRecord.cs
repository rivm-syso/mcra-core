using MCRA.Utils.Statistics;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryDistributionRouteCompoundRecord {
        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [Description("Route exposure.")]
        [DisplayName("Exposure Route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of an exposure route x substance combination to the nondietary exposure, including membership probabilities.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a substance and route to exposure.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a substance and route to exposure.")]
        [DisplayName("Lower bound (%) (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of a substance and route to exposure.")]
        [DisplayName("Upper bound (%) (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfDays { get; set; }

        [Description("Mean exposure for a route on all individual days (acute) or individuals (chronic).")]
        [DisplayName("Mean exposure all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Total { get { return Mean * Percentage / 100; } }

        [Description("Median for all exposures of nondietary exposure route x substance combination values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Lower percentile point for all exposures of nondietary exposure route x substance combination values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("{LowerPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Upper percentile point for all exposuresof nondietary exposure route x substance combination values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("{UpperPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) with exposure.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Average for exposure > 0 of nondietary exposure route x substance combination values.")]
        [DisplayName("Mean {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("Median percentile for exposure > 0 of nondietary exposure route x substance combination values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median {IndividualDayUnit} exposure > 0(IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Lower percentile point for exposure > 0 of nondietary exposure route x substance combination values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("{LowerPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Upper percentile point for exposure > 0 of nondietary exposure route x substance combination values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("{UpperPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }

        [Description("Relative potency factor. Exposures are unscaled.")]
        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double RelativePotencyFactor { get; set; }
    }
}

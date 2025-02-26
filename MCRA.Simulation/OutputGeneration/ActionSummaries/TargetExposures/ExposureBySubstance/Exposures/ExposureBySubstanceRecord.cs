using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for substances, relative contribution to the upper exposure distribution.
    /// </summary>
    public sealed class ExposureBySubstanceRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividuals { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) with exposure > 0.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Mean exposure for a substance on all individual days (acute) or individuals (chronic).")]
        [DisplayName("Mean exposure for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("p50 percentile of all exposure values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Description("Average exposure value, for exposures > 0 (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Mean for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("p50 percentile, for exposures > 0 (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point, for exposures > 0 of exposure values (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point, for exposures > 0 of exposure values (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings).")]
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

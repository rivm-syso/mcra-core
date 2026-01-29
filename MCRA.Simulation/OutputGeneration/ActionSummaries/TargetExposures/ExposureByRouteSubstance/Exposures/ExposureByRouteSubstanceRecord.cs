using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureByRouteSubstanceRecord {

        [Description("External exposure route")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0.")]
        [DisplayName("{IndividualDayUnit} with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividuals { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) with exposure.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Mean exposure for a substance and route on all individual days (acute) or individuals (chronic).")]
        [DisplayName("Mean exposure all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("p50 percentile of all exposure values (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point of all exposure values (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Description("Average exposure value, for exposures > 0 (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Mean {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("p50 percentile, for exposures > 0 (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Median {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point, for exposures > 0 of exposure values (expressed per substance [not in equivalents of reference substance])  (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point, for exposures > 0 of exposure values (expressed per substance [not in equivalents of reference substance]) (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }

        [Description("Relative potency factor. RPFs are not applied.")]
        [DisplayName("RPF")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double RelativePotencyFactor { get; set; }

        [Description("Assessment group membership.")]
        [DisplayName("P(AG)")]
        [DisplayFormat(DataFormatString = "{0:G2}")]
        public double AssessmentGroupMembership { get; set; }
    }
}

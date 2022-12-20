using MCRA.Utils.Statistics;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodAsMeasuredSubstanceProcessingTypeRecord {


        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Food name.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("Food code.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Processing type code.")]
        [DisplayName("Processing type code")]
        public string ProcessingTypeCode { get; set; }

        [Description("Processing type.")]
        [DisplayName("Processing type")]
        public string ProcessingTypeName { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of a food to exposure.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a food to exposure.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a food to exposure.")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of a food to exposure.")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfPositives { get; set; }

        [Description("Mean exposure for a food, substance and processing type combination on all individual days (acute) or individuals (chronic) (expressed per substance [not in equivalents of reference substance]).")]
        [DisplayName("Mean exposure all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("Average for exposures > 0 (expressed per food substance combination [not in equivalents of reference substance]).")]
        [DisplayName("Mean exposure for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("Processing factor.")]
        [DisplayName("Processing factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProcessingFactor { get; set; }

        [Description("Processing correction factor.")]
        [DisplayName("Processing correction factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProcessingCorrectionFactor { get; set; }

    }
}

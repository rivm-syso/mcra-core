﻿using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for modelled foods, relative contribution to the exposure/threshold value distribution.
    /// </summary>
    public sealed class ExposureThresholdRatioModelledFoodRecord  {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of a food to exposure/threshold value.")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Description("Mean relative contribution of a food to the exposure/threshold value.")]
        [DisplayName("Contribution (%) mean")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MeanContribution { get { return Contributions.Any() ? Contributions.Average() : double.NaN; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a food to the exposure/threshold value.")]
        [DisplayName("Contribution (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(UncertaintyLowerBound); } }

        [Description("Upper uncertainty bound relative contribution of a food to the exposure/threshold value.")]
        [DisplayName("Contribution (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(UncertaintyUpperBound); } }

        [Description("Number of days with exposure/threshold value for acute or number of individuals for chronic.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? N { get; set; }

        [Description("Mean exposure/threshold value for a food on all individual days (acute) or individuals (chronic).")]
        [DisplayName("Mean for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Total { get; set; }

        [Description("p50 percentile of all exposure/threshold value values.")]
        [DisplayName("Median for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point of all exposure/threshold value values (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point of all exposure/threshold value values (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        /// <summary>
        /// NOTE interpretation changed: variable Zero is now the complement meaning NOT Zero
        /// </summary>
        [Display(AutoGenerateField = false)]
        public double Zero { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) with exposure/threshold value.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ZeroPercentage { get { return Zero * 100; } }

        [Description("Average exposure/threshold value, for exposure > 0.")]
        [DisplayName("Mean for {IndividualDayUnit} exposure > 0 (IntakeUnit).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("p50 percentile exposure/threshold value for exposure > 0 ")]
        [DisplayName("Median for {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point for exposure/threshold value for exposure > 0 (default 25%, see Output settings) of the exposure/threshold value distribution.")]
        [DisplayName("{LowerPercentage} for {IndividualDayUnit}  exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point for exposure/threshold value for exposure > 0 (default 75%, see Output settings) of the exposure/threshold value distribution.")]
        [DisplayName("{UpperPercentage} for {IndividualDayUnit}  exposure > 0 (IntakeUnit))")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }

        [Description("Number of substances.")]
        [DisplayName("Number of substances included")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int? NumberOfSubstances { get; set; }
    }
}
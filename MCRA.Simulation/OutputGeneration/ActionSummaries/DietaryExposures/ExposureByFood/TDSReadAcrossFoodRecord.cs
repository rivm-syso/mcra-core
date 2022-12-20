using MCRA.Utils.Statistics;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for modelled foods, relative contribution to the upper tail of the exposure distribution.
    /// </summary>
    public sealed class TDSReadAcrossFoodRecord {
        [Description("Food name: 'all TDS foods' reflects the total exposure of all foods that enter the exposure distribution through a TDS composition, all other foods are converted through a read across translation.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }
        
        [Description("Translation: route is TDS composition or Read Across translation")]
        [DisplayName("Translation")]
        public string Translation { get; set; }

        [Display(AutoGenerateField = false)]
        public double Contribution { get; set; }

        [Description("Relative contribution of a food to exposure")]
        [DisplayName("Contribution (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double ContributionPercentage { get { return Contribution * 100; } }

        [Display(AutoGenerateField = false)]
        public List<double> Contributions { get; set; }

        [Description("Lower uncertainty bound relative contribution of a food to exposure.")]
        [DisplayName("Lower bound (%) (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double LowerContributionPercentage { get { return Contributions.Percentile(2.5); } }

        [Description("Upper uncertainty bound relative contribution of a food to exposure.")]
        [DisplayName("Upper bound (%) (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double UpperContributionPercentage { get { return Contributions.Percentile(97.5); } }

        [Description("Food as eaten is linked to a TDS food using a Read Across translation (name)")]
        [DisplayName("Linked to TDS food name")]
        public string TDSFoodName { get; set; }

        [Description("Food as eaten is linked to a TDS food using a Read Across translation (code)")]
        [DisplayName("Linked to TDS food code")]
        public string TDSFoodCode { get; set; }
    }
}

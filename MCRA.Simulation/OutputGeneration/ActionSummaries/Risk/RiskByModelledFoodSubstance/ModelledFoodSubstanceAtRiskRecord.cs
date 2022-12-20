using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for modelled food x substances.
    /// </summary>
    public sealed class ModelledFoodSubstanceAtRiskRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is exceeded specifically due to the modelled food x substance.")]
        [DisplayName("At risk due to food x substance (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AtRiskDueToModelledFoodSubstance { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is not exceeded.")]
        [DisplayName("Not at risk (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double NotAtRisk { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is exceeded already by contributions from other modelled foods x substances.")]
        [DisplayName("At risk with or without food x substance(%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AtRiskWithOrWithout { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for substances.
    /// </summary>
    public sealed class SubstanceAtRiskRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is exceeded specifically due to the substance.")]
        [DisplayName("At risk due to substance (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AtRiskDueToSubstance { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is not exceeded.")]
        [DisplayName("Not at risk (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double NotAtRisk { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is exceeded already by contributions from other substances.")]
        [DisplayName("At risk with or without substance (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AtRiskWithOrWithout { get; set; }




    }
}

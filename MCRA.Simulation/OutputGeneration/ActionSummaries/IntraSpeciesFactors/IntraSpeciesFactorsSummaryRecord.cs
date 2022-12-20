using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IntraSpeciesFactorsSummaryRecord {
        [Description("effect")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("effect")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("compound")]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("compound")]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("specifies variation between humans in the population")]
        [DisplayName("Intra-species lower limit")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double LowerVariationFactor { get; set; }

        [Description("specifies the uncertainty in the intraspecies factor (lower limit)")]
        [DisplayName("Intra-species upper limit")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double UpperVariationFactor { get; set; }
    }
}

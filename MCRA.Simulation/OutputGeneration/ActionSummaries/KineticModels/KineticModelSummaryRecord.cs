using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticModelSummaryRecord {

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("The kinetic model used for the specified substance.")]
        [DisplayName("Model")]
        public string Model { get; set; }

        [Description("The species to which this model applies.")]
        [DisplayName("Species")]
        public string Species { get; set; }

        //Dont think these properties are useful (to echo in summary)
        //[Description("Absorption factor dietary exposure.")]
        //[DisplayName("Absorption factor dietary exposure")]
        //[DisplayFormat(DataFormatString = "{0:G3}")]
        //public double AbsorptionFactorDietaryExposure { get; set; }

        //[Description("Absorption factor oral exposure.")]
        //[DisplayName("Absorption factor oral exposure")]
        //[DisplayFormat(DataFormatString = "{0:G3}")]
        //public double AbsorptionFactorOralExposure { get; set; }

        //[Description("Absorption factor dermal exposure.")]
        //[DisplayName("Absorption factor dermal exposure")]
        //[DisplayFormat(DataFormatString = "{0:G3}")]
        //public double AbsorptionFactorDermalExposure { get; set; }

        //[Description("Absorption factor inhalation exposure.")]
        //[DisplayName("Absorption factor inhalation exposure")]
        //[DisplayFormat(DataFormatString = "{0:G3}")]
        //public double AbsorptionFactorInhalationExposure { get; set; }

    }
}

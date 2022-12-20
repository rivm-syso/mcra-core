using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AbsorptionFactorRecord {

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string Route { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Absorption factor")]
        [DisplayName("Absorption factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AbsorptionFactor { get; set; }

        [DisplayName("Default")]
        public string IsDefault { get; set; }
    }
}

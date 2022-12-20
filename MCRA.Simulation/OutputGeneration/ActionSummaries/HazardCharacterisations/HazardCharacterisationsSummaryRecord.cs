using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsSummaryRecord : HazardCharacterisationSummaryRecord {

        [Display(AutoGenerateField = false)]
        public string HazardCharacterisationType { get; set; }

        [Display(AutoGenerateField = false)]
        public double NominalInterSpeciesConversionFactor { get ; set;}
     
        [Display(AutoGenerateField = false)]
        public double NominalIntraSpeciesConversionFactor { get; set; }
    }
}

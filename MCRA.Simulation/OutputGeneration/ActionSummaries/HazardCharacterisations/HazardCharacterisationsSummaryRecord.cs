using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsSummaryRecord : HazardCharacterisationsSummaryRecordBase {

        [Display(AutoGenerateField = false, Order = 100)]
        public string HazardCharacterisationType { get; set; }

        [Display(AutoGenerateField = false, Order = 100)]
        public double NominalInterSpeciesConversionFactor { get ; set;}

        [Display(AutoGenerateField = false, Order = 100)]
        public double NominalIntraSpeciesConversionFactor { get; set; }
    }
}

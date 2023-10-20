using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ImputedHazardCharacterisationSummaryRecord : HazardCharacterisationsSummaryRecordBase {

        [Display(Name = "Cramer class", Order = 100)]
        public int? CramerClass{ get; set; }

        [Description("Nominal intra-species conversion factor to translate the test system hazard characterisation to a human hazard characterisation (1/EFintra).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Intra-species conversion factor", Order = 100)]
        public double NominalIntraSpeciesConversionFactor { get; set; }

    }
}

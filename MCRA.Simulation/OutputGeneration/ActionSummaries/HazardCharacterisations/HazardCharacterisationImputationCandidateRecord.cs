using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationImputationRecord : AvailableHazardCharacterisationsSummaryRecord {

        [Display(Name = "Cramer class", Order = 99)]
        public int? CramerClass{ get; set; }

    }
}

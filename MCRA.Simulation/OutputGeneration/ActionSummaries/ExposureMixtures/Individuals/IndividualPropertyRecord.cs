using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.ExposureMixtures {
    public sealed class IndividualPropertyRecord : PopulationCharacteristicsDataRecord {
        [Description("Population and subgroup")]
        [DisplayName("Group")]
        public string Group { get; set; }

        [Description("Number of individuals")]
        [DisplayName("Number of individuals")]
        public int Number { get; set; }

        
    }
}

using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDietStudyCompositionsSummaryRecord {

        [DisplayName("Number of TDS foods")]
        public int Number { get; set; }

        [DisplayName("Number of sub foods")]
        public int NumberOfSubFoods { get; set; }
    }
}

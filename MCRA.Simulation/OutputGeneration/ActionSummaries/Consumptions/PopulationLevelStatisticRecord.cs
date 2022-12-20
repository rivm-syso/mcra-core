using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PopulationLevelStatisticRecord {

        [DisplayName("Level")]
        public string Level { get; set; }

        [DisplayName("Frequency")]
        public double Frequency { get; set; }

    }
}

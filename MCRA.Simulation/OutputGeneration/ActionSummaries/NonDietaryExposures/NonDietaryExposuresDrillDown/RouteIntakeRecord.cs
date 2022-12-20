using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public class RouteIntakeRecord {
        [DisplayName("Exposure route")]
        public string Route { get; set; }

        [DisplayName("Exposure")]
        public double Exposure { get; set; }

        [DisplayName("Absorption factor")]
        public double AbsorptionFactor { get; set; }
    }
}

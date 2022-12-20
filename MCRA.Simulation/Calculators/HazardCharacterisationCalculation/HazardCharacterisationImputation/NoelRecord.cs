using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {
    public class NoelRecord {
        public int CramerClass { get; set; }
        public string Species { get; set; }
        public ExposureRouteType ExposureRoute { get; set; }
        public string AdministrationType { get; set; }
        public double Noel { get; set; }
    }
}

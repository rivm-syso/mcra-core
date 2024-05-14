using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation {
    public class NoelRecord {
        public int CramerClass { get; set; }
        public string Species { get; set; }
        public ExposureRoute ExposureRoute { get; set; }
        public string AdministrationType { get; set; }
        public double Noel { get; set; }
        public TargetUnit TargetUnit => TargetUnit.FromExternalDoseUnit(DoseUnit, ExposureRoute);
        public DoseUnit DoseUnit { get; } = DoseUnit.mgPerKgBWPerDay;
    }
}

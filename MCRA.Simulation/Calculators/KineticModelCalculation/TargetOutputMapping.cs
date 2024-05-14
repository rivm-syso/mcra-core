using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class TargetOutputMapping {

        public string CompartmentId { get; set; }
        public string SpeciesId { get; set; }
        public Compound Substance { get; set; }
        public TargetUnit OutputUnit { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public KineticModelOutputType OutputType { get; set; }

        public double GetUnitAlignmentFactor(double compartmentWeight) {
            return OutputUnit.GetAlignmentFactor(
                TargetUnit, Substance.MolecularMass, compartmentWeight
            );
        }
    }
}

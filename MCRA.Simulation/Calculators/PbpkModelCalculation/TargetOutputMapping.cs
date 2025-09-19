using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation {
    public class TargetOutputMapping {
        public string CompartmentId { get; set; }
        public string SpeciesId { get; set; }
        public Compound Substance { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public KineticModelOutputDefinition OutputDefinition { get; set; }

        public TargetUnit OutputUnit {
            get {
                return OutputDefinition.TargetUnit;
            }
        }

        public KineticModelOutputType OutputType {
            get {
                return OutputDefinition.Type;
            }
        }

        public double GetUnitAlignmentFactor(double compartmentWeight) {
            return OutputUnit.GetAlignmentFactor(
                TargetUnit, Substance.MolecularMass, compartmentWeight
            );
        }
    }
}

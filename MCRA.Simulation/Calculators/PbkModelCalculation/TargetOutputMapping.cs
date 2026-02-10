using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;

namespace MCRA.Simulation.Calculators.PbkModelCalculation {
    public class TargetOutputMapping {
        // Target, unit and substance
        public TargetUnit TargetUnit { get; set; }
        public Compound Substance { get; set; }

        public string OutputId { get; set; }
        public string CompartmentId { get; set; }
        public TargetUnit OutputUnit { get; set; }
        public PbkModelOutputType OutputType { get; set; }

        public double GetUnitAlignmentFactor(double compartmentWeight) {
            return OutputUnit.GetAlignmentFactor(
                TargetUnit, Substance.MolecularMass, compartmentWeight
            );
        }
    }
}

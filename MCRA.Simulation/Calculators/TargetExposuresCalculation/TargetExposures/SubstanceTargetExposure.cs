using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class SubstanceTargetExposure : ISubstanceTargetExposure {

        public SubstanceTargetExposure() { }

        public SubstanceTargetExposure(Compound substance, double substanceAmount) {
            Substance = substance;
            SubstanceAmount = substanceAmount;
        }

        public Compound Substance { get; set; }

        public TargetUnit Unit { get; private set; }

        public double SubstanceAmount { get; set; }


        public (string compartment, double relativeCompartmentWeight) CompartmentInfo { get; set; }

        public double EquivalentSubstanceAmount(double rpf, double membershipProbability) {
            return SubstanceAmount * rpf * membershipProbability;
        }
    }
}

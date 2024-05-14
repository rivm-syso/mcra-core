using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public class SubstanceTargetExposure : ISubstanceTargetExposure {

        public SubstanceTargetExposure() { }

        public SubstanceTargetExposure(Compound substance, double substanceAmount) {
            Substance = substance;
            Exposure = substanceAmount;
        }

        public Compound Substance { get; set; }

        public double Exposure { get; set; }

        public double EquivalentSubstanceExposure(double rpf, double membershipProbability) {
            return Exposure * rpf * membershipProbability;
        }
    }
}

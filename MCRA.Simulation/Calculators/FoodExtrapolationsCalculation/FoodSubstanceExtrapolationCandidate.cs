using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class FoodSubstanceExtrapolationCandidate {
        public Food ExtrapolationFood { get; set; }
        public Compound MeasuredSubstance { get; set; }
    }
}

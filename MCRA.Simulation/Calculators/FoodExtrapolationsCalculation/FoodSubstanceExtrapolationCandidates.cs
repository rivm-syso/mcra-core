using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class FoodSubstanceExtrapolationCandidates {
        public Food Food { get; set; }
        public Compound Substance { get; set; }
        public int Measurements { get; set; }
        public Dictionary<Food, List<FoodSubstanceExtrapolationCandidate>> PossibleExtrapolations { get; set; }

        public FoodSubstanceExtrapolationCandidates() {
            PossibleExtrapolations = new Dictionary<Food, List<FoodSubstanceExtrapolationCandidate>>();
        }
    }
}

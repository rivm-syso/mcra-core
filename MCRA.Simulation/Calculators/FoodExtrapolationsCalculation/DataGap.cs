using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class DataGap {
        public Food Food { get; set; }
        public Compound Substance { get; set; }
        public int Measurements { get; set; }
    }
}

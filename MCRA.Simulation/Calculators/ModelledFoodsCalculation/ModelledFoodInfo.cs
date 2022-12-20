using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ModelledFoodsCalculation {

    public class ModelledFoodInfo {

        public Food Food { get; set; }

        public Compound Substance { get; set; }

        public bool HasMeasurements { get; set; }

        public bool HasPositiveMeasurements { get; set; }

        public bool HasMrl { get; set; }
    }
}

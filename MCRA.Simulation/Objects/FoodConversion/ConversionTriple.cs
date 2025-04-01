
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Objects {

    /// <summary>
    /// Combination of ID's for food as eaten and modelled food
    /// including the conversion steps taken by the conversion algorithm.
    /// </summary>
    public sealed class FoodConversionTriple {
        public Food FoodAsEaten { get; set; }
        public Food FoodAsMeasured { get; set; }
        public Compound Compound { get; set; }
    }
}

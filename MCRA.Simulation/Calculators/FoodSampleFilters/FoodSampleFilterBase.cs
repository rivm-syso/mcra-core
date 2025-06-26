using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.FoodSampleFilters {
    public abstract class FoodSampleFilterBase {

        /// <summary>
        /// Returns whether the given food sample passes the filter.
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public abstract bool Passes(FoodSample foodSample);
    }
}

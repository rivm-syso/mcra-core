using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public abstract class FoodSampleFilterBase : IFilter<FoodSample> {

        /// <summary>
        /// Returns whether the given food sample passes the filter.
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public abstract bool Passes(FoodSample foodSample);
    }
}

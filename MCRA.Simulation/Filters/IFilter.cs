namespace MCRA.Simulation.Filters {

    /// <summary>
    /// Generic interface for object filters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFilter<T> {

        /// <summary>
        /// Returns whether the given item passes the filter.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Passes(T item);
    }
}

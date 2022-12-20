namespace MCRA.Utils.Statistics.Histograms {

    /// <summary>
    /// A tuple value, used for storing the contribution by category of a
    /// categorized histogram bin object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CategoryContribution<T> {

        /// <summary>
        /// The category to which this contribution applies.
        /// </summary>
        public T Category { get; set; }

        /// <summary>
        /// The contribution fraction of this category relative to the total
        /// of all categories.
        /// </summary>
        public double Contribution { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CategoryContribution() {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="category"></param>
        /// <param name="contribution"></param>
        public CategoryContribution(T category, double contribution) {
            Category = category;
            Contribution = contribution;
        }
    }
}

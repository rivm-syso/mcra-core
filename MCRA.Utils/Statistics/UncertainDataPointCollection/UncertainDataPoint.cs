namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Represents an x-y coordinate with an additional list for uncertain y values
    /// </summary>
    /// <typeparam name="T">The type of the x-axis</typeparam>
    public sealed class UncertainDataPoint<T> {

        private List<double> _uncertaintyValues = [];

        public T XValue { get; set; }

        /// <summary>
        /// A reference value of the uncertainty values (e.g., nominal value)
        /// </summary>
        public double ReferenceValue { get; set; }

        /// <summary>
        /// The list of uncertainty values
        /// </summary>
        public List<double> UncertainValues {
            get {
                return this._uncertaintyValues;
            }
            set {
                this._uncertaintyValues = value;
            }
        }

        /// <summary>
        /// Gets the specified percentile of the UncertaintyValues.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public double Percentile(double percentage) {
            if (this.UncertainValues.Count != 0) {
                return this.UncertainValues.Percentile(percentage);
            } else {
                return double.NaN;
            }
        }

        /// <summary>
        /// The 50% percentile of the UncertaintyValues
        /// </summary>
        public double MedianUncertainty {
            get {
                if (this.UncertainValues.Count != 0) {
                    return this.UncertainValues.Percentile(50);
                } else {
                    return double.NaN;
                }
            }
        }
    }
}

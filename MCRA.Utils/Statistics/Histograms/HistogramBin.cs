namespace MCRA.Utils.Statistics.Histograms {

    /// <summary>
    /// Represents a bin of a histogram.
    /// </summary>
    public class HistogramBin : ICloneable {

        /// <summary>
        /// The lower bound or starting point of the bin.
        /// </summary>
        public double XMinValue { get; set; }

        /// <summary>
        /// The upper bound or endpoint of the bin.
        /// </summary>
        public double XMaxValue { get; set; }

        /// <summary>
        /// The frequency count of the bin.
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// The bin's centrer point.
        /// </summary>
        public double XMidPointValue {
            get {
                return (XMaxValue + XMinValue) / 2;
            }
        }

        /// <summary>
        /// The interval length or width of the bin.
        /// </summary>
        public double Width {
            get {
                return XMaxValue - XMinValue;
            }
        }

        #region IClonable
        public object Clone() {
            return this.MemberwiseClone();
        }
        #endregion
        
    }
}

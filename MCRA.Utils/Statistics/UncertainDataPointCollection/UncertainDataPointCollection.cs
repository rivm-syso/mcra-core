using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Represents a collection of uncertain data points.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class UncertainDataPointCollection<T> : List<UncertainDataPoint<T>> {

        /// <summary>
        /// Constructor
        /// </summary>
        public UncertainDataPointCollection() 
            : base() {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xValues"></param>
        public UncertainDataPointCollection(IEnumerable<T> xValues) 
            : base() {
                this.XValues = xValues;
        }

        /// <summary>
        /// The collection as an enumerable of uncertain data points
        /// </summary>
        public IEnumerable<T> XValues {
            get {
                return this.Select(c => c.XValue).ToList();
            }
            set {
                if (this.Count == 0) {
                    foreach (var v in value) {
                        this.Add(new UncertainDataPoint<T>() {
                            XValue = v,
                        });
                    }
                } else {
                    if (this.Count != value.Count()) {
                        throw new InvalidOperationException("The argument count must match the source count.");
                    } else {
                        var valueList = value.ToList();
                        for (int i = 0; i < this.Count; i++) {
                            this[i].XValue = valueList[i];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the reference values of the uncertain data points.
        /// </summary>
        public IEnumerable<double> ReferenceValues {
            get {
                return this.Select(c => c.ReferenceValue).ToList();
            }
            set {
                if (this.Count != value.Count()) {
                    throw new InvalidOperationException("The argument count must match the source count.");
                } else {
                    var valueList = value.ToList();
                    for (int i = 0; i < this.Count; i++) {
                        this[i].ReferenceValue = valueList[i];
                    }
                }
            }
        }

        /// <summary>
        /// Adds uncertainty values to this uncertain data point collection.
        /// </summary>
        /// <param name="uncValues"></param>
        public void AddUncertaintyValues(IEnumerable<double> uncValues) {
            var uncValuesList = uncValues.ToList();
            if (this.Count != uncValuesList.Count) {
                throw new InvalidOperationException("The argument count must match the source count.");
            } else {
                for (int i = 0; i < this.Count; i++) {
                    this[i].UncertainValues.Add(uncValuesList[i]);
                }
            }
        }
    }
}

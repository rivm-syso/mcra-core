using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Compiled.Wrappers {

    public sealed class SingleValueConsumptionModel {

        /// <summary>
        /// The consumed base food (i.e., without processing/facets).
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// The processing type(s) associated with this food consumption.
        /// </summary>
        public ICollection<ProcessingType> ProcessingTypes { get; set; }

        /// <summary>
        /// Factor to express an intake of the consumed raw product to an intake
        /// of the processed product.
        /// </summary>
        public double ProcessingCorrectionFactor { get; set; }

        /// <summary>
        /// Mean consumption amount.
        /// </summary>
        public double MeanConsumption { get; set; } = double.NaN;

        /// <summary>
        /// Large portion amount.
        /// </summary>
        public double LargePortion { get; set; } = double.NaN;

        /// <summary>
        /// Mean body weight.
        /// </summary>
        public double BodyWeight { get; set; } = double.NaN;
        /// <summary>
        /// Percentiles.
        /// </summary>
        public List<(double Percentage, double Percentile)> Percentiles { get; set; }

        /// <summary>
        /// Tries to get the specified percentile. Returns NaN when this
        /// percentile is not available.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public double GetPercentile(double percentage) {
            if (Percentiles?.Any(r => r.Percentage == percentage) ?? false) {
                return Percentiles.First(r => r.Percentage == percentage).Percentile;
            }
            return double.NaN;
        }
    }
}

using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Class for recording single-value concentrations for a combination of food and substance.
    /// E.g., mean/median residue values, MRL, LOQ, etc.
    /// </summary>
    public class SingleValueConcentrationModel {

        /// <summary>
        /// The food-as-measured.
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// The substance.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// Mean concentration (positives).
        /// </summary>
        public double MeanConcentration { get; set; } = double.NaN;

        /// <summary>
        /// Highest concentration (positives).
        /// </summary>
        public double HighestConcentration { get; set; } = double.NaN;

        /// <summary>
        /// LOQ.
        /// </summary>
        public double Loq { get; set; } = double.NaN;

        /// <summary>
        /// LOQ.
        /// </summary>
        public double Mrl { get; set; } = double.NaN;

        /// <summary>
        /// Percentiles.
        /// </summary>
        public List<(double, double)> Percentiles { get; set; }

        /// <summary>
        /// Tries to get the specified percentile. Returns NaN when this
        /// percentile is not available.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public double GetPercentile(double percentage) {
            if (Percentiles?.Any(r => r.Item1 == percentage) ?? false) {
                return Percentiles.First(r => r.Item1 == percentage).Item2;
            }
            return double.NaN;
        }

        /// <summary>
        /// Tries to get the specified percentile and sets the out parameter
        /// accordingly. Sets the output parameter to NaN when no value has
        /// been found.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns>True if a value has been found. Otherwise false.</returns>
        public bool TryGetPercentile(double percentage, out double percentile) {
            if (Percentiles?.Any(r => r.Item1 == percentage) ?? false) {
                percentile = Percentiles.First(r => r.Item1 == percentage).Item2;
                return true;
            }
            percentile = double.NaN;
            return false;
        }

        /// <summary>
        /// Returns whether this single value concentration has measuremment
        /// data associated with it.
        /// </summary>
        /// <returns></returns>
        public bool HasMeasurement() {
            return !double.IsNaN(Loq) || HasPositiveMeasurement();
        }

        /// <summary>
        /// Returns whether there is any positive single concentration value.
        /// I.e., no LOQ/MRL.
        /// </summary>
        /// <returns></returns>
        public bool HasPositiveMeasurement() {
            return !double.IsNaN(HighestConcentration)
                || !double.IsNaN(MeanConcentration)
                || (Percentiles?.Any(r => !double.IsNaN(r.Item2)) ?? false);
        }

        /// <summary>
        /// Creates a copy/clone of this single-value concentration.
        /// </summary>
        /// <returns></returns>
        public SingleValueConcentrationModel Clone() {
            return new SingleValueConcentrationModel() {
                Food = Food,
                Substance = Substance,
                HighestConcentration = HighestConcentration,
                MeanConcentration = MeanConcentration,
                Loq = Loq,
                Mrl = Mrl,
                Percentiles = Percentiles?.Select(r => (r.Item1, r.Item2)).ToList()
            };
        }
    }
}

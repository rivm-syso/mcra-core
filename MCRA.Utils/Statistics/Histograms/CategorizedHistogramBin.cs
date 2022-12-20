using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Statistics.Histograms {

    /// <summary>
    /// Represents a bin of a histogram that includes contribution of the different categories
    /// that contribute to the values of this bin.
    /// </summary>
    public sealed class CategorizedHistogramBin<T> : HistogramBin {

        private List<CategoryContribution<T>> _contributionFractions;

        /// <summary>
        /// The contribution factors of the different categories that contribute
        /// to the values of this bin.
        /// </summary>
        public List<CategoryContribution<T>> ContributionFractions {
            get {
                if (_contributionFractions == null) {
                    _contributionFractions = new List<CategoryContribution<T>>();
                }
                return _contributionFractions;
            }
            set {
                var sum = value.Sum(i => i.Contribution);
                _contributionFractions = new List<CategoryContribution<T>>();
                foreach (var item in value) {
                    _contributionFractions.Add(new CategoryContribution<T>(item.Category, item.Contribution / sum));
                }
            }
        }
    }
}

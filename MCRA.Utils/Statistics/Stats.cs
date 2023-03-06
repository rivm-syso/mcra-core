using System.ComponentModel.DataAnnotations;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Extension methods for statistics of IEnumerable objects.
    /// </summary>
    public static class Stats {

        /// <summary>
        /// This is a comparer for tuples which finds the tuple by comparing on Item1 of a tuple
        /// </summary>
        public class TupleItem1Comparer : IComparer<(double, double)> {
            public static readonly TupleItem1Comparer Instance = new();
            public int Compare((double, double) x, (double, double) y) {
                return x.Item1.CompareTo(y.Item1);
            }
        }

        /// <summary>
        /// Returns the weighted mean for the input array
        /// </summary>
        /// <param name="values"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static double Average(this IEnumerable<double> values, IEnumerable<double> weights) {
            var count = values.Count();
            var sum = 0D;
            if (weights != null && weights.Count() == count) {
                for (int i = 0; i < count; i++) {
                    sum += values.ElementAt(i) * weights.ElementAt(i);
                }
                return sum / weights.Sum();
            } else {
                return values.Average();
            }
        }

        /// <summary>
        /// Returns the mean for the input array. Skips NaN.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double AverageNotNaN(this IEnumerable<double> values) {
            return values.Where(v => !double.IsNaN(v)).Average();
        }

        /// <summary>
        /// Returns the population variance (divide by (n-1)) for the array. Skips NaN.
        /// </summary>
        /// <param name="values">The list of values.</param>
        /// <returns>The variance.</returns>
        public static double Variance(this IEnumerable<double> values) {
            var xnn = values.Where(v => !double.IsNaN(v)).ToList();
            if (xnn.Count > 1) {
                var mean = xnn.Average();
                var count = xnn.Count;
                return xnn.Sum(v => (v - mean).Squared()) / (count - 1);
            }
            return double.NaN;
        }

        /// <summary>
        /// Returns the weighted variance for the input array
        /// </summary>
        /// <param name="values"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static double Variance(this IEnumerable<double> values, IEnumerable<double> weights) {
            var mean = values.Average(weights);
            var count = values.Count();
            if (count > 1) {
                var sum = 0D;
                for (int i = 0; i < count; i++) {
                    sum += Math.Pow((values.ElementAt(i) - mean), 2) * weights.ElementAt(i);
                }
                return sum / (weights.Sum() - 1);
            }
            return double.NaN;
        }

        /// <summary>
        /// Returns the median.
        /// </summary>
        public static double Median(this IEnumerable<double> values) {
            return values.Percentile(50);
        }

        /// <summary>
        /// Get the percentiles of an unsorted array. Does not extrapolate beyond min and max.
        /// Example to calculate the min, 25% point, the median, the 75% point and the max: 
        /// double percentages[] = new double[] {0, 25, 50, 75, 100} 
        /// double[] percentiles = Percentiles(X, percentages)
        /// </summary>
        /// <param name="X">Array from which percentiles are to be calculated</param>
        /// <param name="percentages">Array with the percentages</param>
        /// <returns>The associated percentiles</returns>
        public static double[] Percentiles(this IEnumerable<double> X, IEnumerable<double> percentages) {
            var Xsorted = X.SortWithoutNaNs();
            var weights = Enumerable.Repeat(1D, Xsorted.Count()).ToList();
            return Xsorted.PercentilesWithSamplingWeights(weights, percentages.ToArray());
        }

        /// <summary>
        /// Get the percentiles of an unsorted array. Does not extrapolate beyond min and max.
        /// Example to calculate the min, 25% point, the median, the 75% point and the max: 
        /// double percentages[] = new double[] {0, 25, 50, 75, 100} 
        /// double[] percentiles = Percentiles(X, percentages)
        /// </summary>
        /// <param name="X">Array from which percentiles are to be calculated</param>
        /// <param name="percentages">Array with the percentages</param>
        /// <returns>The associated percentiles</returns>
        public static double[] Percentiles(this IEnumerable<double> X, params double[] percentages) {
            return X.Percentiles(percentages.AsEnumerable());
        }

        /// <summary>
        /// Get a single percentile of an unsorted array. Does not extrapolate beyond min and max.
        /// Example to calculate the median: 
        /// Use Percentiles(X,perc) if you want multiple percentiles, which is much faster than one by one
        /// </summary>
        /// <param name="X">Array from which percentiles are to be calculated</param>
        /// <param name="perc">The percentage</param>
        /// <returns>The associated percentile</returns>
        public static double Percentile(this IEnumerable<double> X, double perc) {
            var Xsorted = X.SortWithoutNaNs();
            return PercentileSorted(Xsorted, perc);
        }

        /// <summary>
        /// Returns a sorted array while ignoring (deleting) NaN's.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<double> SortWithoutNaNs(this IEnumerable<double> source) {
            return source.Where(c => !double.IsNaN(c)).Select(c => c).OrderBy(c => c);
        }

        /// <summary>
        /// Get a single percentile of a sorted array. Does not extrapolate beyond min and max.
        /// </summary>
        /// <param name="X">Array from which percentiles are to be calculated</param>
        /// <param name="percentage">The percentage</param>
        /// <returns>The associated percentile</returns>
        public static double PercentileSorted(this IEnumerable<double> X, double percentage) {
            if ((percentage < 0) || (percentage > 100)) {
                throw new ArgumentOutOfRangeException("Requested percentage should be in range [0,100]");
            }
            if (!X.Any()) {
                return double.NaN;
            }
            var n = X.Count();
            var dx = percentage / 100 * (n - 1);
            var ix = BMath.Floor(dx);
            if ((dx - ix) > 0) {
                var w = dx - ix;
                return (1 - w) * X.ElementAt(ix) + w * X.ElementAt(ix + 1);
            } else {
                return X.ElementAt(ix);
            }
        }

        /// <summary>
        ///  Returns unweighted percentages (output) based on percentiles (input). Does extrapolate beyond min and max.
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="limits"></param>
        /// <returns></returns>
        public static double[] Percentages(this IEnumerable<double> xValues, IEnumerable<double> limits) {
            var x = xValues.Where(c => !double.IsNaN(c)).Select(c => c).ToList();
            var weights = Enumerable.Repeat(1D, x.Count).ToList();
            var cumulativeWeights = weights.CumulativeWeights(c => c / x.Count).ToList();
            var xSorted = x.OrderBy(c => c).ToList();
            var interpolatingPoints = getInterpolationPercentages(xSorted, cumulativeWeights);
            var result = new List<double>();
            foreach (var limit in limits) {
                result.Add(getPercentage(interpolatingPoints, limit));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns weighted percentages (output) based on percentiles (input)
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="weights"></param>
        /// <param name="limits"></param>
        /// <returns></returns>
        public static double[] PercentagesWithSamplingWeights(this IEnumerable<double> xValues, List<double> weights, IEnumerable<double> limits) {
            var n = xValues.Count();
            var weightsSum = weights.Sum();
            var samplingWeights = weights.Select(c => c * n / weightsSum).ToList();
            var data = xValues.Where(c => !double.IsNaN(c))
                .Zip(samplingWeights, (x, w) => (x, w))
                .OrderBy(c => c.x);
            var X = data.Select(c => c.x).ToList();
            var W = data.Select(c => c.w).ToList();
            var samplingWeightsSum = samplingWeights.Sum();
            var cumulativeWeights = W.CumulativeWeights(c => c / samplingWeightsSum).ToList();
            var interpolatingPoints = getInterpolationPercentages(X, cumulativeWeights);
            var result = new List<double>();
            foreach (var limit in limits) {
                result.Add(getPercentage(interpolatingPoints, limit));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Evaluates whether unweighted or weighted percentages (output) are requested based on percentiles (input)
        /// Returns for WEIGHTS EQUAL to 1 or NULL unweighted percentages (output) based on percentiles (input)
        /// Returns for WEIGHTS UNEQUAL to 1 weighted percentages (output) based on percentiles (input)
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="weights"></param>
        /// <param name="limits"></param>
        /// <returns></returns>
        public static double[] PercentagesWithSamplingWeights(this IEnumerable<double> xValues, List<double> weights, double[] limits) {
            if (weights != null) {
                return xValues.PercentagesWithSamplingWeights(weights, limits.AsEnumerable());
            }
            return xValues.Percentages(limits.ToList());
        }

        /// <summary>
        /// Returns interpolating points for weighted percentages
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="cumulativeWeights"></param>
        /// <returns></returns>
        private static List<(double, double)> getInterpolationPercentages(List<double> xValues, List<double> cumulativeWeights) {
            var results = new List<(double, double)>(xValues.Count + 1);
            var minimum = xValues.Count == 0 ? 0 : xValues.Min();
            results.Add((0D, minimum));
            for (int i = 0; i < xValues.Count; i++) {
                results.Add((cumulativeWeights[i], xValues[i]));
            }
            return results;
        }

        /// <summary>
        /// Computes percentiles with additional zeros.
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="weights"></param>
        /// <param name="percentages"></param>
        /// <param name="sumWeightZeros"></param>
        /// <returns></returns>
        public static double[] PercentilesAdditionalZeros(
            this IEnumerable<double> xValues,
            List<double> weights,
            double[] percentages,
            double sumWeightZeros
        ) {
            if ((xValues?.Count() ?? 0) != weights.Count) {
                throw new Exception("Length of weights differs from lenght of xValues");
            }
            if (sumWeightZeros < 1) {
                var x = xValues.Append(0).ToList();
                var w = weights.Append(sumWeightZeros).ToList();
                return x.PercentilesWithSamplingWeights(w, percentages.AsEnumerable());
            } else {
                var x = xValues.Concat(Enumerable.Repeat(0D, 2));
                var w = weights.Concat(new List<double>() { sumWeightZeros - 1D, 1D }).ToList();
                return x.PercentilesWithSamplingWeights(w, percentages.AsEnumerable());
            }
        }

        /// <summary>
        /// Evaluates whether unweighted or weighted percentiles (output) are requested based on percentages (input)
        /// Returns for WEIGHTS UNEQUAL to 1 or NULL weighted percentiles (output) based on percentages (input)
        /// Returns for WEIGHTS EQUAL to 1 unweighted percentiles (output) based on percentages (input)
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="weights"></param>
        /// <param name="percentages"></param>
        /// <returns></returns>
        public static double[] PercentilesWithSamplingWeights(this IEnumerable<double> xValues, List<double> weights, double[] percentages) {
            if (weights != null) {
                if ((xValues?.Count() ?? 0) != weights.Count) {
                    throw new Exception("Length of weights differs from lenght of xValues");
                }
                return xValues.PercentilesWithSamplingWeights(weights, percentages.AsEnumerable());
            }
            return xValues.Percentiles(percentages.AsEnumerable());
        }

        /// <summary>
        /// Evaluates whether unweighted or weighted percentiles (output) are requested based on percentages (input)
        /// Returns for WEIGHTS UNEQUAL to 1 or NULL weighted percentiles (output) based on percentages (input)
        /// Returns for WEIGHTS EQUAL to 1 percentiles (output) based on percentages (input)
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="weights"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public static double PercentilesWithSamplingWeights(this IEnumerable<double> xValues, List<double> weights, double percentage) {
            if (weights != null) {
                return SinglePercentileWithSamplingWeights(xValues.ToArray(), weights, percentage, 0D);
            }
            var x = xValues.OrderBy(c => c).ToArray();
            return x.PercentileSorted(percentage);
        }

        /// <summary>
        /// Returns weighted percentiles (output)
        /// http://www.mathworks.nl/matlabcentral/fileexchange/16920-returns-weighted-percentiles-of-a-sample/content/wprctile.m
        /// Type 4: p(k) = k/n. 
        ///         That is, linear interpolation of the empirical cdf. 
        /// Type 5: p(k) = (k-0.5)/n. 
        ///         That is a piecewise linear function where the knots are the values midway through the steps of the empirical cdf. This is popular amongst hydrologists. 
        /// Type 6: p(k) = k/(n+1). 
        ///         Thus p(k) = E[F(x[k])]. This is used by Minitab and by SPSS. 
        /// Type 7: p(k) = (k-1)/(n-1). 
        ///         In this case, p(k) = mode[F(x[k])]. This is used by S. 
        /// Type 8: p(k) = (k-1/3)/(n+1/3). 
        ///         Then p(k) =~ median[F(x[k])]. The resulting quantile estimates are approximately median-unbiased regardless of the distribution of x. 
        /// Type 9: p(k) = (k-3/8)/(n+1/4). 
        ///         The resulting quantile estimates are approximately unbiased for the expected order statistics 
        ///         if x is normally distributed.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="weights"></param>
        /// <param name="percentages"></param>
        /// <returns></returns>
        public static double[] PercentilesWithSamplingWeights(this IEnumerable<double> x, List<double> weights, IEnumerable<double> percentages) {
            foreach (var percentage in percentages) {
                if ((percentage < 0) || (percentage > 100)) {
                    throw new ArgumentOutOfRangeException("Requested percentage should be in range [0,100]");
                }
            }
            var xValues = x.ToList();
            if (!xValues.Any()) {
                return percentages.Select(r => 0D).ToArray();
            } else if (xValues.Count == 1) {
                return percentages.Select(r => xValues[0]).ToArray();
            } else if (xValues.Distinct().Count() == 1) {
                return percentages.Select(r => xValues[0]).ToArray();
            }
            var interpolatingPoints = getInterpolationPercentiles(xValues, weights);
            var result = percentages
                .Select(percentage => getPercentile(interpolatingPoints, percentage / 100))
                .ToArray();
            return result;
        }

        /// <summary>
        /// Returns only one weighted percentile (output) based on percentage (input)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="weights"></param>
        /// <param name="percentage"></param>
        /// <param name="defaultIfEmpty"></param>
        /// <returns></returns>
        public static double SinglePercentileWithSamplingWeights(this IEnumerable<double> x, List<double> weights, double percentage, double defaultIfEmpty) {
            if ((percentage < 0) || (percentage > 100)) {
                throw new ArgumentOutOfRangeException("Requested percentage should be in range [0,100]");
            }
            var xValues = x.ToList();
            if (!xValues.Any()) {
                return defaultIfEmpty;
            } else if (xValues.Count == 1) {
                return xValues[0];
            }
            var interpolatingPoints = getInterpolationPercentiles(xValues, weights);
            return getPercentile(interpolatingPoints, percentage / 100);
        }

        /// <summary>
        /// Returns at which percentage of the sorted array we can find the first value greater than limit.
        /// </summary>
        /// <param name="interpolatingPoints"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static double getPercentage(List<(double Percentage, double Value)> interpolatingPoints, double limit) {
            var first = (Percentage:0D, Value: 0D);
            var last = (Percentage: 0D, Value: double.NaN);

            var array = interpolatingPoints.Select(c => c.Value).ToArray();
            //Find index based on binary search
            var ix = Array.BinarySearch(array, limit);
            if (ix < 0) {
                ix = ~ix;
            }
            if (ix == 0) {
                first = interpolatingPoints.First();
            } else if (ix == array.Length) {
                return 100;
            } else {
                first = interpolatingPoints.ElementAt(ix - 1);
                last = interpolatingPoints.ElementAt(ix);
            }

            var dx = (limit - first.Value) / (last.Value - first.Value);
            if (double.IsNaN(dx)) {
                return first.Percentage * 100;
            }
            return (first.Percentage + dx * (last.Percentage - first.Percentage)) * 100;
        }

        /// <summary>
        /// Returns interpolating points for weighted percentiles
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        private static List<(double, double)> getInterpolationPercentiles(List<double> xValues, List<double> weights) {
            if (weights == null) {
                weights = Enumerable.Repeat(1D, xValues.Count).ToList();
            }

            var weightsSum = 0D;
            var values = xValues.Zip(weights, (x, w) => (Value: x, Weight: w))
                .OrderBy(x => x.Value)
                .ThenByDescending(x => x.Weight)
                .Select(g => {
                    weightsSum += g.Weight;
                    return (g.Value, g.Weight);
                })
                .ToArray();

            var n = values.Length;
            //convert weights to their relative part in the sum of weights
            //use a running sum
            var cumulativeWeights = new double[n];
            var runningSum = 0d;
            for (int i = 0; i < n; i++) {
                var relativeWeight = values[i].Weight * n / weightsSum;
                values[i].Weight = relativeWeight;
                cumulativeWeights[i] = runningSum += relativeWeight;
            }
            var results = new List<(double, double)>(n);
            var pkMax = getPK(cumulativeWeights.Last(), values.Last().Weight, n, quantileAlgorithm.Type4);
            for (int i = 0; i < n; i++) {
                var pk = getPK(cumulativeWeights[i], values[i].Weight, n, quantileAlgorithm.Type4);
                results.Add((pk / pkMax, values[i].Value));
            }
            return results;
        }

        /// <summary>
        /// Returns weighted percentiles based on interpolation
        /// </summary>
        /// <param name="interpolatingPoints"></param>
        /// <param name="fraction"></param>
        /// <returns></returns>
        private static double getPercentile(List<(double Percentile, double Value)> interpolatingPoints, double fraction) {
            var first = (Percentile: 0D, Value: 0D);
            var last = (Percentile: 0D, Value: 0D);
            //Find index based on binary search
            var ix = interpolatingPoints.BinarySearch((fraction, 0D), TupleItem1Comparer.Instance);
            if (ix < 0) {
                ix = ~ix;
            }
            if (ix == 0) {
                first = interpolatingPoints.First();
            } else if (ix == interpolatingPoints.Count) {
                last = interpolatingPoints.Last();
            } else {
                first = interpolatingPoints.ElementAt(ix - 1);
                last = interpolatingPoints.ElementAt(ix);
            }
            var dx = (fraction - first.Percentile) / (last.Percentile - first.Percentile);
            return first.Value + dx * (last.Value - first.Value);
        }

        /// <summary>
        /// Selecting one of the 6 quantile algorithms for weighted percentiles
        /// Type 4: p(k) = k/n. 
        ///         That is, linear interpolation of the empirical cdf. 
        /// Type 5: p(k) = (k-0.5)/n. 
        ///         That is a piecewise linear function where the knots are the values midway through the steps of the empirical cdf. This is popular amongst hydrologists. 
        /// Type 6: p(k) = k/(n+1). 
        ///         Thus p(k) = E[F(x[k])]. This is used by Minitab and by SPSS. 
        /// Type 7: p(k) = (k-1)/(n-1). 
        ///         In this case, p(k) = mode[F(x[k])]. This is used by S. 
        /// Type 8: p(k) = (k-1/3)/(n+1/3). 
        ///         Then p(k) =~ median[F(x[k])]. The resulting quantile estimates are approximately median-unbiased regardless of the distribution of x. 
        /// Type 9: p(k) = (k-3/8)/(n+1/4) (Blom scores)
        ///         The resulting quantile estimates are approximately unbiased for the expected order statistics 
        ///         if x is normally distributed.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="sortedW"></param>
        /// <param name="n"></param>
        /// <param name="quantileAlgorithm"></param>
        /// <returns></returns>
        private static double getPK(double k, double sortedW, int n, quantileAlgorithm quantileAlgorithm) {
            switch (quantileAlgorithm) {
                case quantileAlgorithm.Type4:
                    return k / n;
                case quantileAlgorithm.Type5:
                    return (k - sortedW) / 2 / n;
                case quantileAlgorithm.Type6:
                    return k / (n + 1);
                case quantileAlgorithm.Type7:
                    return (k - sortedW) / (n - 1);
                case quantileAlgorithm.Type8:
                    return (k - sortedW / 3) / (n + 1D / 3);
                case quantileAlgorithm.Type9:
                    return (k - sortedW * 3D / 8) / (n + 1D / 4);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// For weighted percentiles
        /// </summary>
        private enum quantileAlgorithm {
            [Display(Name = "Algorithm4")]
            Type4,
            [Display(Name = "Algorithm5")]
            Type5,
            [Display(Name = "Algorithm6")]
            Type6,
            [Display(Name = "Algorithm7")]
            Type7,
            [Display(Name = "Algorithm8")]
            Type8,
            [Display(Name = "Algorithm9")]
            Type9,
        }

        /// <summary>
        /// Interpolates in arrays X (should be sorted) and Y (xin --> Interpolations)
        /// </summary>
        /// <param name="Xsorted"></param>
        /// <param name="Y"></param>
        /// <param name="xin"></param>
        /// <returns></returns>
        public static double[] Interpolations(double[] Xsorted, double[] Y, double[] xin) {
            var yout = new double[xin.Length];
            var w = 0D;
            for (int i = 0; i < xin.Length; i++) {
                var j = 1;
                while (xin[i] > Xsorted[j]) {
                    j++;
                    if (j == Xsorted.Length - 1) {
                        break;
                    }
                }
                w = (xin[i] - Xsorted[j - 1]) / (Xsorted[j] - Xsorted[j - 1]);
                yout[i] = Y[j - 1] + w * (Y[j] - Y[j - 1]);
            }
            return yout;
        }

        /// <summary>
        /// Calculates adapted Blom-scores, the minimum number = 2.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<double> Blom(double a, int n) {
            var z = Blom(n);
            if (n < 2) {
                throw new Exception("For adapted Blom scores the minimum value for n = 2");
            }
            z[0] = z[0] * a;
            z[1] = z[1] * a;
            z[n - 2] = z[n - 2] * a;
            z[n - 1] = z[n - 1] * a;
            return z;
        }

        /// <summary>
        /// Calculates regular Blom-scores.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<double> Blom(int n) {
            var z = new List<double>();
            for (int i = 0; i < n; i++) {
                z.Add(NormalDistribution.InvCDF(0, 1, (i + 1 - 3D / 8D) / (n + 1D / 4D)));
            }
            return z;
        }

        /// <summary>
        /// Normalizes the input list w based on the mean of the values in the list.
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static IEnumerable<double> Normalize(IEnumerable<double> weights) {
            if (!weights.Any()) {
                return new List<double>();
            }
            var normalWeights = new List<double>();
            var meanWeights = weights.Average();
            foreach (var w in weights) {
                normalWeights.Add(w / meanWeights);
            }
            return normalWeights;
        }

        /// <summary>
        /// Standardizes the values of the list, i.e., corrects by mu and stddev: (x - mu) / stdev.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<double> Standardize(this IEnumerable<double> values) {
            var mean = values.Average();
            var stDev = Math.Sqrt(values.Variance());
            var xStandardize = new List<double>();
            for (int i = 0; i < values.Count(); i++) {
                xStandardize.Add((values.ElementAt(i) - mean) / stDev);
            }
            return xStandardize;
        }
    }
}

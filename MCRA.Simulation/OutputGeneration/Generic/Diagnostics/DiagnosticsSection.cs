using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Generic.Diagnostics {
    public sealed class DiagnosticsSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<SigmaSizeRecord> MCSigmas { get; set; }
        public List<SigmaSizeRecord> BootstrapSigmas { get; set; }

        public int BootstrapSize { get; set; }
        public int NumberOfUncertaintyRuns { get; set; }

        private Dictionary<int, UncertainDataPointCollection<double>> _uncertainDataPointDictionary = new();

        private int _minimalSampleSize = 50;
        /// <summary>
        /// Split nominal intakes in 2, 4, 8,... sets of size n = N/2^p, p = 1, 2, 3,....
        /// Estimate percentiles in each set and calculate for each division p the variance of the percentiles.
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="percentages"></param>
        public void Summarize(
            List<double> intakes,
            List<double> weights,
            double[] percentages,
            int bootstrapSize = 0,
            int numberOfUncertaintyRuns = 0
        ) {
            BootstrapSize = bootstrapSize;
            NumberOfUncertaintyRuns = numberOfUncertaintyRuns;
            var maxP = Math.Floor(Math.Log(intakes.Count / 1d / _minimalSampleSize) / Math.Log(2)) + 1;
            if (maxP > 1) {
                MCSigmas = new List<SigmaSizeRecord>();
                for (int p = 1; p < maxP; p++) {
                    var percentiles = new UncertainDataPointCollection<double>() { XValues = percentages };
                    var n = Math.Pow(2, p);
                    var size = (int)Math.Floor(intakes.Count / n);
                    for (int i = 0; i < n; i++) {
                        var sample = intakes.Skip(i * size).Take(size).ToList();
                        var sampleWeights = weights?.Skip(i * size).Take(size).ToList() ?? null;
                        percentiles.AddUncertaintyValues(sample.PercentilesWithSamplingWeights(sampleWeights, percentages));
                    }
                    MCSigmas.AddRange(calculateSigma(percentiles, size));
                }
            }
        }

        /// <summary>
        /// In each bootstrap run, take complete set of intakes, take 1/2, 1/4, 1/8 of the set. Estimate in each set the requested percentiles.
        /// Do this for all subsequent bootstrap runs and collect the percentiles in the dictionary. The key represents the sample size.
        /// At the end, for each key a collection of data points is available representing the percentage with a list of estimated
        /// percentiles (conditional on the sample size).
        /// So each key (sample size) contains: an XValue (= percentage e.g. 50) and a list of percentile values of size b = number of uncertainty runs
        ///                                     an XValue (= percentage e.g. 95) and a list of percentile values of size b = number of uncertainty runs
        ///                                     an XValue (= percentage e.g. 99) and a list of percentile values of size b = number of uncertainty runs
        /// Keys are e.g. 100,000; 50,000; 25,000; 12,500 etc.
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="percentages"></param>
        public void SummarizeUncertainty(
            List<double> intakes,
            List<double> weights,
            double[] percentages
        ) {
            var maxP = Math.Floor(Math.Log(intakes.Count / 1d / _minimalSampleSize) / Math.Log(2)) + 1;
            if (maxP > 1) {
                for (int p = 0; p < maxP; p++) {
                    var n = Math.Pow(2, p);
                    var size = (int)Math.Floor(intakes.Count / n);
                    var sample = intakes.Take(size).ToList();
                    var sampleWeights = weights?.Take(size).ToList() ?? null;
                    UncertainDataPointCollection<double> uncertaintDataPointCollection;
                    if (_uncertainDataPointDictionary.TryGetValue(size, out uncertaintDataPointCollection)) {
                        uncertaintDataPointCollection.AddUncertaintyValues(sample.PercentilesWithSamplingWeights(sampleWeights, percentages));
                    } else {
                        uncertaintDataPointCollection = new UncertainDataPointCollection<double>() { XValues = percentages };
                        uncertaintDataPointCollection.AddUncertaintyValues(sample.PercentilesWithSamplingWeights(sampleWeights, percentages));
                        _uncertainDataPointDictionary[size] = uncertaintDataPointCollection;
                    }
                }

                BootstrapSigmas = new List<SigmaSizeRecord>();
                foreach (var key in _uncertainDataPointDictionary.Keys) {
                    BootstrapSigmas.AddRange(calculateSigma(_uncertainDataPointDictionary[key], key));
                }
            }
        }

        private List<SigmaSizeRecord> calculateSigma(UncertainDataPointCollection<double> percentiles, int size) {
            return percentiles.Select(c => new SigmaSizeRecord() {
                Percentage = c.XValue,
                Size = size,
                NumberOfValues = c.UncertainValues.Count,
                Sigma = Math.Sqrt(c.UncertainValues.Variance())
            }).ToList();
        }

        /// <summary>
        /// Not used but will be in the future
        /// </summary>
        /// <param name="mcVariances"></param>
        /// <param name="uncertaintyVariances"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        private double getInterpolate(List<SigmaSizeRecord> mcVariances, List<SigmaSizeRecord> uncertaintyVariances, double percentage) {
            var results = mcVariances.Where(c => c.Percentage == percentage).ToList();
            var bootStrapResult = uncertaintyVariances.Single(c => c.Percentage == percentage);
            var point1 = results.First(c => c.Size <= bootStrapResult.Size);
            var point2 = results.LastOrDefault(c => c.Size > bootStrapResult.Size);
            if (point2 == null) {
                return point1.Sigma;
            } else {
                var delta = Convert.ToDouble(bootStrapResult.Size - point1.Size) / Convert.ToDouble(point2.Size - point1.Size);
                return point1.Sigma - delta * (point1.Sigma - point2.Sigma);
            }
        }
    }
}

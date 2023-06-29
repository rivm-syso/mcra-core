using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using System.Collections.Concurrent;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CompoundExposureDistributionsSection : SummarySection {

        public CompoundExposureDistributionRecord CombinedCompoundExposureDistributionRecord { get; set; }
        public List<CompoundExposureDistributionRecord> CompoundExposureDistributionRecords { get; set; }

        public double Upper { get; set; }
        public double Lower { get; set; }
        public double MaximumFrequency { get; set; }
        public bool EqualityOfMeans { get; set; }
        public bool HomogeneityOfVariances { get; set; }

        public void Summarize(
                Dictionary<Compound, List<ExposureRecord>> exposurePerCompoundRecords,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                bool isPerPerson
            ) {
            SummarizeExposureDistributionPerCompound(
                exposurePerCompoundRecords,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );
            calculateStatistics();
        }

        /// <summary>
        /// Summarize exposure distributions per substance
        /// </summary>
        /// <param name="exposurePerCompoundRecords"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeExposureDistributionPerCompound(
            Dictionary<Compound, List<ExposureRecord>> exposurePerCompoundRecords,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var allLogIntakes = new List<double>();
            var allSamplingWeights = new List<double>();
            var logIntakesBag = new ConcurrentBag<List<double>>();
            var weightsBag = new ConcurrentBag<List<double>>();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1000 }; //, CancellationToken = cancelToken };
            var exposureArray = exposurePerCompoundRecords.ToArray();
            var exposureDistributionPerCompoundRecords = new CompoundExposureDistributionRecord[exposureArray.Length];

            Parallel.For(0, exposureArray.Length, parallelOptions, idx => {
                var c = exposureArray[idx];
                var result = c.Value.Where(i => i.ExposurePerBodyWeight > 0).ToList();
                var logIntakes = result.Select(i => Math.Log(i.ExposurePerBodyWeight * (isPerPerson ? i.BodyWeight : 1))).ToList();
                var sampleWeights = result.Select(i => i.SamplingWeight).ToList();
                List<HistogramBin> intakeDistributionBins = null;
                var mu = double.NaN;
                var sigma = double.NaN;
                var percentiles = new double[2];
                if (logIntakes.Count > 0) {
                    var numberOfBins = Math.Sqrt(logIntakes.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logIntakes.Count)) : 100;
                    intakeDistributionBins = logIntakes.MakeHistogramBins(sampleWeights, numberOfBins, logIntakes.Min(), logIntakes.Max());
                    mu = logIntakes.Average(sampleWeights);
                    sigma = Math.Sqrt(logIntakes.Variance(sampleWeights));
                    percentiles = logIntakes.PercentilesWithSamplingWeights(sampleWeights, new double[] { 50, 99 });
                }
                logIntakesBag.Add(logIntakes);
                weightsBag.Add(sampleWeights);
                exposureDistributionPerCompoundRecords[idx] = new CompoundExposureDistributionRecord() {
                    CompoundCode = c.Key.Code,
                    CompoundName = c.Key.Name,
                    Percentage = 100 - result.Sum(w => w.SamplingWeight) / c.Value.Sum(w => w.SamplingWeight) * 100d,
                    N = logIntakes.Count,
                    Mu = mu,
                    Sigma = sigma,
                    HistogramBins = intakeDistributionBins,
                    RelativePotencyFactor = relativePotencyFactors?[c.Key] ?? double.NaN,
                    AssessmentGroupMembership = membershipProbabilities?[c.Key] ?? double.NaN
                };
            });
            CompoundExposureDistributionRecords = exposureDistributionPerCompoundRecords
                .OrderBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ToList();

            allLogIntakes = logIntakesBag.SelectMany(b => b).ToList();
            allSamplingWeights = weightsBag.SelectMany(b => b).ToList();

            var allPercentiles = allLogIntakes.PercentilesWithSamplingWeights(allSamplingWeights, new double[2] { 50, 99 });
            var allNumberOfBins = Math.Sqrt(allLogIntakes.Count) < 100 ? BMath.Ceiling(Math.Sqrt(allLogIntakes.Count)) : 100;
            if (exposureDistributionPerCompoundRecords.Any()) {
                CombinedCompoundExposureDistributionRecord = new CompoundExposureDistributionRecord() {
                    CompoundName = "All substances",
                    CompoundCode = "All substances",
                    Percentage = exposureDistributionPerCompoundRecords.Average(c => c.Percentage),
                    N = exposureDistributionPerCompoundRecords.Sum(c => c.N),
                    RelativePotencyFactor = double.NaN,
                    AssessmentGroupMembership = double.NaN
                };
                if (allLogIntakes.Count > 2) {
                    CombinedCompoundExposureDistributionRecord.Mu = allLogIntakes.Average(allSamplingWeights);
                    CombinedCompoundExposureDistributionRecord.Sigma = Math.Sqrt(allLogIntakes.Variance(allSamplingWeights));
                    CombinedCompoundExposureDistributionRecord.HistogramBins = allLogIntakes.MakeHistogramBins(allSamplingWeights, allNumberOfBins, allLogIntakes.Min(), allLogIntakes.Max());
                }
            }
        }

        /// <summary>
        /// Calculate statistics for exposure distributions
        /// </summary>
        private void calculateStatistics() {
            var result = CompoundExposureDistributionRecords
                .Where(c => c.HistogramBins != null)
                .ToList();

            if (result.Any()) {
                Upper = Math.Exp(result.Select(c => c.Mu + 3 * c.Sigma).Max());
                Lower = Math.Exp(result.Select(c => c.Mu - 3 * c.Sigma).Min());
                MaximumFrequency = result.Select(c => c.HistogramBins.Any() ? c.HistogramBins.Max(m => m.Frequency) : double.NaN).Max();
                var mu = result.Select(c => c.Mu).ToList();
                var sigma = result.Select(c => c.Sigma).ToList();
                var numberOfObservations = result.Select(c => c.N).ToList();
                if (mu.Count > 1) {
                    try {
                        var resultMeans = StatisticalTests.EqualityOfMeans(mu, sigma, numberOfObservations);
                        EqualityOfMeans = resultMeans.Probability < 0.05;
                        var resultVariances = StatisticalTests.HomogeneityOfVariances(sigma, numberOfObservations);
                        HomogeneityOfVariances = resultVariances.Probability < 0.05;
                    } catch {
                        // If failed; too bad!
                    }
                }
            }
        }
    }
}

using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionSubstanceSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;
        public List<DistributionSubstanceRecord> Records { get; set; }
        protected double[] Percentages { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        /// <summary>
        /// Note that contributions are always rescaled
        /// </summary>
        /// <param name="aggregateExposures"></param>
        /// <param name="substances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="externalExposureUnit"></param>
        /// <returns></returns>
        public List<DistributionSubstanceRecord> Summarize(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();

            var allWeights = aggregateExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var sumSamplingWeights = allWeights.Sum();

            var result = new List<DistributionSubstanceRecord>();
            foreach (var substance in substances) {
                var exposures = aggregateExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            externalExposureUnit.IsPerUnit()
                        )
                    ))
                    .ToList();

                var rpf = relativePotencyFactors?[substance] ?? double.NaN;
                var membership = membershipProbabilities?[substance] ?? 1D;
                var percentilesAll = exposures.Select(c => c.Exposure).PercentilesWithSamplingWeights(allWeights, Percentages);
                var weights = exposures.Where(c => c.Exposure > 0).Select(c => c.SamplingWeight).ToList();
                var percentiles = exposures.Where(c => c.Exposure > 0).Select(c => c.Exposure).PercentilesWithSamplingWeights(weights, Percentages);
                var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
                var record = new DistributionSubstanceRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contributions = [],
                    Contribution = total * rpf * membership,
                    Percentage = weights.Sum() / sumSamplingWeights * 100D,
                    Mean = total / exposures.Sum(c => c.SamplingWeight),
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                    AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                    N = weights.Count,
                };
                result.Add(record);
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return result.OrderByDescending(r => r.Contribution).ToList();
        }

        /// <summary>
        /// Note that contributions are always rescaled
        /// </summary>
        /// <param name="aggregateExposures"></param>
        /// <param name="substances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="kineticConversionFactors"></param>
        /// <param name="externalExposureUnit"></param>
        /// <returns></returns>
        public List<DistributionSubstanceRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var records = new List<DistributionSubstanceRecord>();

            foreach (var substance in substances) {
                var exposures = aggregateExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            externalExposureUnit.IsPerUnit()
                        )
                    ))
                    .ToList();

                var record = new DistributionSubstanceRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contribution = exposures.Sum(c => c.Exposure * c.SamplingWeight)
                        * (relativePotencyFactors?[substance] ?? double.NaN)
                        * membershipProbabilities[substance],
                };
                records.Add(record);
            }
            var rescale = records.Sum(c => c.Contribution);
            records.ForEach(r => r.Contribution = r.Contribution / rescale);
            return records;
        }

        protected void SetUncertaintyBounds(
            List<DistributionSubstanceRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }
        protected void UpdateContributions(List<DistributionSubstanceRecord> records) {
            records = records.Where(r => !double.IsNaN(r.Contribution)).ToList();
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.CompoundCode == record.CompoundCode)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}

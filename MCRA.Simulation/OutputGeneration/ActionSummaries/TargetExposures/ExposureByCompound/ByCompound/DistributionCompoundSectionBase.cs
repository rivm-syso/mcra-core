using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionCompoundSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;
        public List<DistributionCompoundRecord> Records { get; set; }
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
        public List<DistributionCompoundRecord> Summarize(
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

            var result = new List<DistributionCompoundRecord>();
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
                var record = new DistributionCompoundRecord {
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

        protected List<DistributionCompoundRecord> SummarizeDietaryAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var totalDietaryIntake = relativePotencyFactors != null ?
                dietaryIndividualDayIntakes
                .Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var allWeights = dietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var sumSamplingWeights = allWeights.Sum();

            var results = substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = dietaryIndividualDayIntakes
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            ExposurePerMassUnit: c.GetSubstanceTotalExposure(substance) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                        ))
                        .ToList();
                    var percentilesAll = exposures.Select(a => a.ExposurePerMassUnit)
                        .PercentilesWithSamplingWeights(allWeights, Percentages);
                    var weights = exposures.Where(c => c.ExposurePerMassUnit > 0).Select(a => a.SamplingWeight).ToList();
                    var percentiles = exposures.Where(c => c.ExposurePerMassUnit > 0)
                        .Select(a => a.ExposurePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, Percentages);
                    var totalSubstanceIntake = relativePotencyFactors != null
                        ? exposures.Sum(a => a.ExposurePerMassUnit * a.SamplingWeight * relativePotencyFactors[substance] * membershipProbabilities[substance])
                        : double.NaN;
                    return new DistributionCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        Contributions = [],
                        Contribution = totalSubstanceIntake / totalDietaryIntake,
                        Percentage = weights.Sum() / sumSamplingWeights * 100D,
                        Mean = exposures.Sum(a => a.ExposurePerMassUnit * a.SamplingWeight) / exposures.Sum(a => a.SamplingWeight),
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                        AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                        N = exposures.Count(a => a.ExposurePerMassUnit > 0),
                    };
                })
                .ToList();

            var rpfs = relativePotencyFactors != null ? relativePotencyFactors : substances.ToDictionary(c => c, c => 1d);
            var daysWithOtherIntakes = dietaryIndividualDayIntakes.Where(c => c.TotalOtherIntakesPerCompound(rpfs, membershipProbabilities) / (isPerPerson ? 1 : c.Individual.BodyWeight) > 0);

            if (daysWithOtherIntakes.Any()) {
                var daysWithOtherIntakesCount = daysWithOtherIntakes.Count();
                var sum = results.Sum(c => c.Contribution);
                if (sum < 1 && !double.IsNaN(sum)) {
                    var otherIntakes = daysWithOtherIntakes.Select(c => c.TotalOtherIntakesPerCompound(rpfs, membershipProbabilities) / (isPerPerson ? 1 : c.Individual.BodyWeight));
                    var weights = daysWithOtherIntakes.Select(c => c.IndividualSamplingWeight).ToList();
                    var n = otherIntakes.Count();
                    var otherAverageIntakes = otherIntakes.Sum();
                    var percentiles = otherIntakes.PercentilesWithSamplingWeights(weights, Percentages);
                    results.Add(new DistributionCompoundRecord {
                        CompoundCode = "Others",
                        CompoundName = "Others",
                        Contribution = 1 - sum,
                        Percentage = daysWithOtherIntakesCount / (double)dietaryIndividualDayIntakes.Count * 100,
                        Percentile25 = percentiles[0],
                        Median = percentiles[1],
                        Percentile75 = percentiles[2],
                        Mean = otherAverageIntakes / n,
                        RelativePotencyFactor = double.NaN,
                        AssessmentGroupMembership = double.NaN,
                        N = daysWithOtherIntakesCount,
                    });
                }
            }
            results = results.OrderByDescending(r => r.Contribution).ToList();
            results.TrimExcess();
            return results;
        }

        protected List<DistributionCompoundRecord> SummarizeDietaryChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var totalDietaryIntake = relativePotencyFactors != null ?
                dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                .Sum()
                : double.NaN;
            var records = new List<DistributionCompoundRecord>();
            foreach (var substance in substances) {
                var exposures = dietaryIndividualDayIntakes
                    .GroupBy(c => c.SimulatedIndividualId)
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(c => (
                        SamplingWeight: c.First().IndividualSamplingWeight,
                        ExposurePerMassUnit: c.Sum(s => s.GetSubstanceTotalExposure(substance))
                            / c.Count()
                            / (isPerPerson ? 1 : c.First().Individual.BodyWeight)
                    ))
                    .ToList();

                var allWeights = exposures.Select(c => c.SamplingWeight).ToList();
                var percentilesAll = exposures.Select(a => a.ExposurePerMassUnit)
                    .PercentilesWithSamplingWeights(allWeights, Percentages);
                var weights = exposures.Where(c => c.ExposurePerMassUnit > 0)
                    .Select(a => a.SamplingWeight)
                    .ToList();
                var percentiles = exposures.Where(c => c.ExposurePerMassUnit > 0)
                    .Select(a => a.ExposurePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, Percentages);

                var totalSubstanceIntake = exposures.Sum(a => a.ExposurePerMassUnit * a.SamplingWeight);
                if (relativePotencyFactors != null) {
                    totalSubstanceIntake *= relativePotencyFactors[substance];
                }
                if (membershipProbabilities != null) {
                    totalSubstanceIntake *= membershipProbabilities[substance];
                }
                var result = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contributions = [],
                    Contribution = totalSubstanceIntake / totalDietaryIntake,
                    Percentage = Convert.ToDouble(weights.Count) / Convert.ToDouble(allWeights.Count) * 100D,
                    Mean = exposures.Sum(a => a.ExposurePerMassUnit * a.SamplingWeight) / exposures.Sum(a => a.SamplingWeight),
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                    AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                    N = exposures.Count(a => a.ExposurePerMassUnit > 0),
                };
                records.Add(result);
            }
            records = records.OrderByDescending(r => r.Contribution).ToList();
            records.TrimExcess();
            return records;
        }

        public List<DistributionCompoundRecord> SummarizeUncertaintyAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var totalDietaryIntake = relativePotencyFactors != null ?
                dietaryIndividualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var records = new List<DistributionCompoundRecord>();
            foreach (var substance in substances) {
                var exposures = dietaryIndividualDayIntakes
                    .AsParallel()
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        IntakePerBodyWeight: c.GetSubstanceTotalExposure(substance) / (isPerPerson ? 1 : c.Individual.BodyWeight)
                    ))
                    .ToList();
                var result = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contribution = exposures.Sum(a => a.IntakePerBodyWeight * a.SamplingWeight * (relativePotencyFactors?[substance] ?? double.NaN) * membershipProbabilities[substance]) / totalDietaryIntake,
                };
                records.Add(result);
            }
            return records;
        }

        public List<DistributionCompoundRecord> SummarizeUncertaintyChronic(
           ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
           ICollection<Compound> substances,
           IDictionary<Compound, double> relativePotencyFactors,
           IDictionary<Compound, double> membershipProbabilities,
           bool isPerPerson
       ) {
            var totalDietaryIntake = relativePotencyFactors != null ? dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Sum(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                : double.NaN;

            var records = new List<DistributionCompoundRecord>();
            foreach (var substance in substances) {
                var exposures = dietaryIndividualDayIntakes
                    .GroupBy(c => c.SimulatedIndividualId)
                    .AsParallel()
                    .Select(c => (
                        SamplingWeight: c.First().IndividualSamplingWeight,
                        Exposure: c.Sum(s => s.GetSubstanceTotalExposure(substance))
                            / (isPerPerson ? 1 : c.First().Individual.BodyWeight)
                            / c.Count()
                    ))
                    .ToList();

                var result = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contribution = exposures
                        .Sum(a => a.Exposure * a.SamplingWeight
                            * (relativePotencyFactors?[substance] ?? double.NaN)
                            * membershipProbabilities[substance]
                        ) / totalDietaryIntake,
                };
                records.Add(result);
            }
            return records;
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
        public List<DistributionCompoundRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var records = new List<DistributionCompoundRecord>();

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

                var record = new DistributionCompoundRecord {
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
            List<DistributionCompoundRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }
        protected void UpdateContributions(List<DistributionCompoundRecord> records) {
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

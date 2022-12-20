using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionCompoundSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;

        public double _lowerPercentage;
        public double _upperPercentage;

        protected List<DistributionCompoundRecord> Summarize(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntake = relativePotencyFactors != null
                ? aggregateIndividualDayExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var allWeights = aggregateIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var sumSamplingWeights = allWeights.Sum();

            var result = new List<DistributionCompoundRecord>();
            foreach (var substance in substances) {
                var exposures = aggregateIndividualDayExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        ExposurePerMassUnit: c.TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)
                    ))
                    .ToList();
                var rpf = relativePotencyFactors?[substance] ?? double.NaN;
                var membership = membershipProbabilities?[substance] ?? 1D;
                var percentilesAll = exposures.Select(a => a.ExposurePerMassUnit).PercentilesWithSamplingWeights(allWeights, percentages);
                var weights = exposures.Where(a => a.ExposurePerMassUnit > 0).Select(a => a.SamplingWeight).ToList();
                var percentiles = exposures.Where(a => a.ExposurePerMassUnit > 0).Select(a => a.ExposurePerMassUnit).PercentilesWithSamplingWeights(weights, percentages);
                var total = exposures.Sum(a => a.ExposurePerMassUnit * a.SamplingWeight);
                var record = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contributions = new List<double>(),
                    Contribution = exposures.Sum(c => c.ExposurePerMassUnit) * rpf * membership / totalIntake,
                    Percentage = weights.Sum() / sumSamplingWeights * 100D,
                    Mean = total / exposures.Sum(a => a.SamplingWeight),
                    Percentile25 = percentiles[0],
                    Median = percentiles[1],
                    Percentile75 = percentiles[2],
                    Percentile25All = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    Percentile75All = percentilesAll[2],
                    RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                    AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                    N = weights.Count(),
                };
                result.Add(record);
            }

            result = result.OrderByDescending(r => r.Contribution).ToList();
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution /= rescale);
            result.TrimExcess();
            return result;
        }

        public List<DistributionCompoundRecord> Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntake = relativePotencyFactors != null
                ? aggregateIndividualExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var allWeights = aggregateIndividualExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var sumSamplingWeights = allWeights.Sum();

            var result = new List<DistributionCompoundRecord>();
            foreach (var substance in substances) {
                var exposures = aggregateIndividualExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        ExposurePerMassUnit: c.TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)
                    ))
                    .ToList();

                var rpf = relativePotencyFactors?[substance] ?? double.NaN;
                var membership = membershipProbabilities?[substance] ?? 1D;
                var percentilesAll = exposures.Select(c => c.ExposurePerMassUnit).PercentilesWithSamplingWeights(allWeights, percentages);
                var weights = exposures.Where(c => c.ExposurePerMassUnit > 0).Select(c => c.SamplingWeight).ToList();
                var percentiles = exposures.Where(c => c.ExposurePerMassUnit > 0).Select(c => c.ExposurePerMassUnit).PercentilesWithSamplingWeights(weights, percentages);
                var total = exposures.Sum(c => c.ExposurePerMassUnit * c.SamplingWeight);
                var record = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contributions = new List<double>(),
                    Contribution = total * rpf * membership / totalIntake,
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
            result = result.OrderByDescending(r => r.Contribution).ToList();
            result.TrimExcess();
            return result;
        }

        protected List<DistributionCompoundRecord> SummarizeDietaryAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalDietaryIntake = relativePotencyFactors != null ?
                dietaryIndividualDayIntakes
                .Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
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
                        .PercentilesWithSamplingWeights(allWeights, percentages);
                    var weights = exposures.Where(c => c.ExposurePerMassUnit > 0).Select(a => a.SamplingWeight).ToList();
                    var percentiles = exposures.Where(c => c.ExposurePerMassUnit > 0)
                        .Select(a => a.ExposurePerMassUnit)
                        .PercentilesWithSamplingWeights(weights, percentages);
                    var totalSubstanceIntake = relativePotencyFactors != null
                        ? exposures.Sum(a => a.ExposurePerMassUnit * a.SamplingWeight * relativePotencyFactors[substance] * membershipProbabilities[substance])
                        : double.NaN;
                    return new DistributionCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        Contributions = new List<double>(),
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
                        N = exposures.Where(a => a.ExposurePerMassUnit > 0).Count(),
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
                    var percentiles = otherIntakes.PercentilesWithSamplingWeights(weights, percentages);
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
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalDietaryIntake = relativePotencyFactors != null ?
                dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                .Sum()
                : double.NaN;
            var distributionCompoundRecords = new List<DistributionCompoundRecord>();
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };

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
                    .PercentilesWithSamplingWeights(allWeights, percentages);
                var weights = exposures.Where(c => c.ExposurePerMassUnit > 0)
                    .Select(a => a.SamplingWeight)
                    .ToList();
                var percentiles = exposures.Where(c => c.ExposurePerMassUnit > 0)
                    .Select(a => a.ExposurePerMassUnit)
                    .PercentilesWithSamplingWeights(weights, percentages);

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
                    Contributions = new List<double>(),
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
                    N = exposures.Where(a => a.ExposurePerMassUnit > 0).Count(),
                };
                distributionCompoundRecords.Add(result);
            }
            distributionCompoundRecords = distributionCompoundRecords.OrderByDescending(r => r.Contribution).ToList();
            distributionCompoundRecords.TrimExcess();
            return distributionCompoundRecords;
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
            var distributionCompoundRecords = new List<DistributionCompoundRecord>();
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
                distributionCompoundRecords.Add(result);
            }
            return distributionCompoundRecords;
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

            var distributionCompoundRecords = new List<DistributionCompoundRecord>();
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };

            foreach (var substance in substances) {
                var exposures = dietaryIndividualDayIntakes
                    .GroupBy(c => c.SimulatedIndividualId)
                    .AsParallel()
                    .Select(c => (
                        SamplingWeight: c.First().IndividualSamplingWeight,
                        ExposurePerMassUnit: c.Sum(s => s.GetSubstanceTotalExposure(substance)) / (isPerPerson ? 1 : c.First().Individual.BodyWeight) / c.Count()
                    ))
                    .ToList();

                var result = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contributions = new List<double>(),
                    Contribution = exposures.Sum(a => a.ExposurePerMassUnit * a.SamplingWeight * (relativePotencyFactors?[substance] ?? double.NaN) * membershipProbabilities[substance]) / totalDietaryIntake,
                };
                distributionCompoundRecords.Add(result);
            }
            return distributionCompoundRecords;
        }

        public List<DistributionCompoundRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntake = relativePotencyFactors != null ?
                aggregateIndividualDayExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var distributionCompoundRecords = new List<DistributionCompoundRecord>();
            foreach (var substance in substances) {
                var exposures = aggregateIndividualDayExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        ExposureConcentration: c.TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)
                    ))
                    .ToList();

                var record = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contribution = exposures.Sum(c => c.ExposureConcentration * c.SamplingWeight) * (relativePotencyFactors?[substance] ?? double.NaN) * membershipProbabilities[substance] / totalIntake,
                };
                distributionCompoundRecords.Add(record);
            }
            return distributionCompoundRecords;
        }

        public List<DistributionCompoundRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var totalIntake = relativePotencyFactors != null ?
                aggregateIndividualExposures.Sum(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight)
                : double.NaN;
            var distributionCompoundRecords = new List<DistributionCompoundRecord>();
            foreach (var substance in substances) {
                var exposures = aggregateIndividualExposures
                    .AsParallel()
                    .WithCancellation(cancelToken)
                    .Select(c => (
                        SamplingWeight: c.IndividualSamplingWeight,
                        ExposurePerMassUnit: c.TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)
                    ))
                    .ToList();

                var record = new DistributionCompoundRecord {
                    CompoundCode = substance.Code,
                    CompoundName = substance.Name,
                    Contribution = exposures.Sum(c => c.ExposurePerMassUnit * c.SamplingWeight) * (relativePotencyFactors?[substance] ?? double.NaN) * membershipProbabilities[substance] / totalIntake,
                };
                distributionCompoundRecords.Add(record);
            }
            return distributionCompoundRecords;
        }

        /// <summary>
        /// Calculate summary statistics for boxplots dietary exposures acute
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="substances"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        protected List<SubstanceTargetExposurePercentilesRecord> SummarizeBoxPotAcute(
           ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
           ICollection<Compound> substances,
           bool isPerPerson
       ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            return substances
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
                    return calculateTargetExposurePercentiles(substance, exposures);
                }).ToList();
        }

        /// <summary>
        /// Calculate summary statistics for boxplots dietary exposures chronic
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="substances"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        protected List<SubstanceTargetExposurePercentilesRecord> SummarizeBoxPotChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
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
                    return calculateTargetExposurePercentiles(substance, exposures);
                })
                .ToList();
        }

        /// <summary>
        /// Calculate summary statistics for boxplots target exposures chronic
        /// </summary>
        /// <param name="aggregateIndividualExposures"></param>
        /// <param name="substances"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        protected List<SubstanceTargetExposurePercentilesRecord> SummarizeBoxPotChronic(
          ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
          ICollection<Compound> substances,
          bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var aiExp = aggregateIndividualExposures
                .SelectMany(c => c.TargetExposuresBySubstance.Where(r => r.Value.SubstanceAmount > 0).Select(r => r.Value.SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)))
                .ToList();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = aggregateIndividualExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            ExposurePerMassUnit: c.TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)
                        ))
                        .ToList();
                    return calculateTargetExposurePercentiles(substance, exposures);
                }).ToList();
        }

        /// <summary>
        /// Calculate summary statistics for boxplots target exposures acute
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="substances"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        protected List<SubstanceTargetExposurePercentilesRecord> SummarizeBoxPotAcute(
         ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
         ICollection<Compound> substances,
         bool isPerPerson
       ) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var aidExp = aggregateIndividualDayExposures
                .SelectMany(c => c.TargetExposuresBySubstance.Where(r => r.Value.SubstanceAmount > 0).Select(r => r.Value.SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight))).ToList();
            return substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(50)
                .Select(substance => {
                    var exposures = aggregateIndividualDayExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            ExposurePerMassUnit: c.TargetExposuresBySubstance[substance].SubstanceAmount / (isPerPerson ? 1 : c.CompartmentWeight)
                        ))
                        .ToList();
                    return calculateTargetExposurePercentiles(substance, exposures);
                }).ToList();
        }

        /// <summary>
        /// Boxplots 
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="exposures"></param>
        /// <returns></returns>
        private static SubstanceTargetExposurePercentilesRecord calculateTargetExposurePercentiles(
            Compound substance, 
            List<(double SamplingWeight, double ExposurePerMassUnit)> exposures
        ) {
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var weights = exposures.Select(a => a.SamplingWeight).ToList();
            var allAxposures = exposures
                .Select(a => a.ExposurePerMassUnit)
                .ToList();
            var percentiles = allAxposures
                .PercentilesWithSamplingWeights(weights, percentages)
                .Select(c => c)
                .ToList();
            var positives = allAxposures.Where(r => r > 0).ToList();
            return new SubstanceTargetExposurePercentilesRecord() {
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Description = "Modelled",
                Percentiles = percentiles,
                NumberOfPositives = weights.Count,
                Percentage = weights.Count * 100d / exposures.Count
            };
        }
    }
}

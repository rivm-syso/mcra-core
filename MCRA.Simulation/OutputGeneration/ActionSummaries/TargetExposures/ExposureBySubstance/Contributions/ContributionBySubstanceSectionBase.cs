using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ContributionBySubstanceSectionBase : SummarySection {

        public override bool SaveTemporaryData => true;
        public List<ContributionBySubstanceRecord> Records { get; set; }
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
        protected List<ContributionBySubstanceRecord> getContributionsRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = new List<ContributionBySubstanceRecord>();
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
                var weightsAll = exposures
                    .Select(c => c.SamplingWeight)
                    .ToList();

                var weights = exposures
                    .Where(c => c.Exposure > 0)
                    .Select(c => c.SamplingWeight)
                    .ToList();

                var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
                var record = new ContributionBySubstanceRecord {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    Contribution = total * rpf * membership,
                    PercentagePositives = weights.Count / exposures.Count * 100D,
                    RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                    Mean = total / weightsAll.Sum(),
                    NumberOfDays = weights.Count,
                    Contributions = [],
                    UncertaintyLowerBound = uncertaintyLowerBound,
                    UncertaintyUpperBound = uncertaintyUpperBound
                };
                result.Add(record);
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return [.. result.OrderByDescending(r => r.Contribution)];
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
        public List<ContributionBySubstanceRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var result = new List<ContributionBySubstanceRecord>();

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

                var record = new ContributionBySubstanceRecord {
                    SubstanceCode = substance.Code,
                    SubstanceName = substance.Name,
                    Contribution = exposures.Sum(c => c.Exposure * c.SamplingWeight)
                        * (relativePotencyFactors?[substance] ?? double.NaN)
                        * membershipProbabilities[substance],
                };
                result.Add(record);
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(r => r.Contribution = r.Contribution / rescale);
            return result;
        }

        protected void updateContributions(List<ContributionBySubstanceRecord> records) {
            records = records.Where(r => !double.IsNaN(r.Contribution)).ToList();
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.SubstanceCode == record.SubstanceCode)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}

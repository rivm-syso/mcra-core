using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ExternalContributionBySourceSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ExternalContributionBySourceRecord> ContributionRecords { get; set; }

        protected List<ExternalContributionBySourceRecord> SummarizeContributions(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ExternalContributionBySourceRecord>();
            var ids = individualsIds ?? externalExposureCollections.First().ExternalIndividualDayExposures.Select(c => c.SimulatedIndividualId).ToHashSet();
            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures
                    .Where(c => ids.Contains(c.SimulatedIndividualId))
                    .Select(id => (
                        Exposure: id.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        SamplingWeight: id.IndividualSamplingWeight
                    ))
                    .ToList();

                var record = getContributionBySourceRecord(collection.ExposureSource, exposures);
                result.Add(record);
            };
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans
                .Where(c => ids.Contains(c.SimulatedIndividualId))
                .Select(id => (
                    Exposure: id.DietaryIntakePerMassUnit,
                    SamplingWeight: id.IndividualSamplingWeight
                )).ToList();
                var dietaryRecord = getContributionBySourceRecord(ExposureSource.DietaryExposures, oims);
                result.Add(dietaryRecord);
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected List<ExternalContributionBySourceRecord> SummarizeUncertainty(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<ExternalContributionBySourceRecord>();
            var ids = individualsIds ?? externalExposureCollections.First().ExternalIndividualDayExposures.Select(c => c.SimulatedIndividualId).ToHashSet();
            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures
                    .Where(c => ids.Contains(c.SimulatedIndividualId))
                    .Select(id => (
                        Exposure: id.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        SamplingWeight: id.IndividualSamplingWeight
                    ))
                    .ToList();

                var record = new ExternalContributionBySourceRecord {
                    ExposureSource = collection.ExposureSource.GetShortDisplayName(),
                    Contribution = exposures.Sum(c => c.Exposure * c.SamplingWeight),
                };
                result.Add(record);
            };
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans
                .Where(c => ids.Contains(c.SimulatedIndividualId))
                .Select(id => (
                    Exposure: id.DietaryIntakePerMassUnit,
                    SamplingWeight: id.IndividualSamplingWeight
                )).ToList();
                var dietaryRecord = new ExternalContributionBySourceRecord {
                    ExposureSource = ExposureSource.DietaryExposures.GetShortDisplayName(),
                    Contribution = oims.Sum(c => c.Exposure * c.SamplingWeight)
                };
                result.Add(dietaryRecord);
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return result.OrderByDescending(c => c.Contribution).ToList();
        }

        private static ExternalContributionBySourceRecord getContributionBySourceRecord(
            ExposureSource source,
            List<(double Exposure, double SamplingWeight)> exposures
        ) {
            var weightsAll = exposures
                .Select(c => c.SamplingWeight)
                .ToList();
            var weights = exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SamplingWeight)
                .ToList();
            var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
            var record = new ExternalContributionBySourceRecord {
                ExposureSource = source.GetShortDisplayName(),
                Contribution = total,
                Percentage = weights.Count / (double)exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weightsAll.Count,
                Contributions = [],
            };
            return record;
        }

        protected void UpdateContributions(List<ExternalContributionBySourceRecord> records) {
            foreach (var record in ContributionRecords) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}

using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ExternalContributionBySourceSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ExternalContributionBySourceRecord> Records { get; set; }

        public List<ExternalContributionBySourceRecord> getContributionRecords(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            HashSet<int> individualsIds,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ExternalContributionBySourceRecord>();
            var ids = individualsIds
                ?? externalExposureCollections
                    .First()
                    .ExternalIndividualDayExposures
                    .Select(c => c.SimulatedIndividual.Id)
                    .ToHashSet();

            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures
                    .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                    .Select(id => (
                        Exposure: id.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        SamplingWeight: id.SimulatedIndividual.SamplingWeight
                    ))
                    .ToList();

                var record = getContributionBySourceRecord(
                    collection.ExposureSource,
                    exposures,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
                result.Add(record);
            };

            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans
                    .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                    .Select(id => (
                        Exposure: id.DietaryIntakePerMassUnit,
                        SamplingWeight: id.SimulatedIndividual.SamplingWeight
                    )).ToList();
                var dietaryRecord = getContributionBySourceRecord(
                    ExposureSource.Diet,
                    oims,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
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
            var result = new List<ExternalContributionBySourceRecord>();
            var ids = individualsIds
                ?? externalExposureCollections
                    .First()
                    .ExternalIndividualDayExposures
                    .Select(c => c.SimulatedIndividual.Id)
                    .ToHashSet();

            foreach (var collection in externalExposureCollections) {
                var exposures = collection.ExternalIndividualDayExposures
                    .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                    .Select(id => (
                        Exposure: id.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        SamplingWeight: id.SimulatedIndividual.SamplingWeight
                    ))
                    .ToList();

                var record = new ExternalContributionBySourceRecord {
                    ExposureSource = collection.ExposureSource.GetShortDisplayName(),
                    Contribution = exposures.Sum(c => c.Exposure * c.SamplingWeight),
                };
                result.Add(record);
            }
            ;
            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans
                    .Where(c => ids.Contains(c.SimulatedIndividual.Id))
                    .Select(id => (
                        Exposure: id.DietaryIntakePerMassUnit,
                        SamplingWeight: id.SimulatedIndividual.SamplingWeight
                    )).ToList();
                var dietaryRecord = new ExternalContributionBySourceRecord {
                    ExposureSource = ExposureSource.Diet.GetShortDisplayName(),
                    Contribution = oims.Sum(c => c.Exposure * c.SamplingWeight)
                };
                result.Add(dietaryRecord);
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        private static ExternalContributionBySourceRecord getContributionBySourceRecord(
            ExposureSource source,
            List<(double Exposure, double SamplingWeight)> exposures,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
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
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound
            };
            return record;
        }

        protected void UpdateContributions(List<ExternalContributionBySourceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
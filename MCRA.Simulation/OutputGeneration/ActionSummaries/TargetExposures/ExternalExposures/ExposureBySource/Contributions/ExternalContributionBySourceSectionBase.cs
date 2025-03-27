using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ExternalContributionBySourceSectionBase : ExternalExposureBySourceSectionBase {
        public override bool SaveTemporaryData => true;
        public List<ExternalContributionBySourceRecord> Records { get; set; }

        public List<ExternalContributionBySourceRecord> SummarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ExternalContributionBySourceRecord>();

            var exposureSourceCollection = CalculateExposures(
                    externalIndividualExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );

            foreach (var (Source, Exposures) in exposureSourceCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionBySourceRecord(
                        Source,
                        Exposures,
                        uncertaintyLowerBound,
                        uncertaintyUpperBound
                    );
                    result.Add(record);
                }
            }

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected List<ExternalContributionBySourceRecord> SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var result = new List<ExternalContributionBySourceRecord>();
            var exposureSourceCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            foreach (var (Source, Exposures) in exposureSourceCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ExternalContributionBySourceRecord {
                        ExposureSource = Source.GetDisplayName(),
                        Contribution = Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight)
                    };
                    result.Add(record);
                }
            }

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        private static ExternalContributionBySourceRecord getContributionBySourceRecord(
            ExposureSource source,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var weightsAll = exposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var weights = exposures
                .Where(c => c.Exposure > 0)
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
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
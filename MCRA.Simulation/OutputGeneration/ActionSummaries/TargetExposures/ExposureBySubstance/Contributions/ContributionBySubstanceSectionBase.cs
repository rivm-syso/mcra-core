using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ContributionBySubstanceSectionBase : ExposureBySubstanceSectionBase {

        public override bool SaveTemporaryData => true;
        public List<ContributionBySubstanceRecord> Records { get; set; }

        protected List<ContributionBySubstanceRecord> SummarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySubstanceRecord>();
            var exposureCollection = CalculateExposures(
                    externalIndividualExposures,
                    substances,
                    kineticConversionFactors,
                    isPerPerson
                );

            foreach (var (Substance, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionBySubstanceRecord(
                        Substance,
                        Exposures,
                        relativePotencyFactors?[Substance],
                        membershipProbabilities?[Substance],
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

        private static ContributionBySubstanceRecord getContributionBySubstanceRecord(
            Compound substance,
            List<(SimulatedIndividual SimulatedIndividual, double Exposure)> exposures,
            double? rpf,
            double? membership,
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
            var total = exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight)
                * (rpf ?? double.NaN)
                * (membership ?? double.NaN);
            var record = new ContributionBySubstanceRecord {
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Contribution = total,
                PercentagePositives = weights.Count / (double)exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound,
                RelativePotencyFactor = rpf ?? double.NaN
            };
            return record;
        }

        protected List<ContributionBySubstanceRecord> summarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySubstanceRecord>();
            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (Substance, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionBySubstanceRecord {
                        SubstanceName = Substance.Name,
                        SubstanceCode = Substance.Code,
                        Contribution = Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight)
                            * relativePotencyFactors?[Substance] ?? double.NaN
                            * membershipProbabilities?[Substance] ?? double.NaN
                    };
                    result.Add(record);
                }
            }

            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected void UpdateContributions(List<ContributionBySubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.SubstanceCode == record.SubstanceCode)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}

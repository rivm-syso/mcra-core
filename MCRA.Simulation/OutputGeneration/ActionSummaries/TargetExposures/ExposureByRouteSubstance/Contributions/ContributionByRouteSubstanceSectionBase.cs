using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ContributionByRouteSubstanceSectionBase : ExposureByRouteSubstanceSectionBase {
        public override bool SaveTemporaryData => true;

        public List<ContributionByRouteSubstanceRecord> Records { get; set; }

        public List<ContributionByRouteSubstanceRecord> SummarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionByRouteSubstanceRecord>();
            var exposureCollection = CalculateExposures(
                    externalIndividualExposures,
                    substances,
                    kineticConversionFactors,
                    isPerPerson
                );

            foreach (var (Route, Substance, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionByRouteSubstanceRecord(
                        Route,
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

        private static ContributionByRouteSubstanceRecord getContributionByRouteSubstanceRecord(
            ExposureRoute Route,
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
            var record = new ContributionByRouteSubstanceRecord {
                ExposureRoute = Route.GetShortDisplayName(),
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

        protected List<ContributionByRouteSubstanceRecord> summarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = new List<ContributionByRouteSubstanceRecord>();
            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (Route, Substance, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionByRouteSubstanceRecord {
                        ExposureRoute = Route.GetDisplayName(),
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

        protected void UpdateContributions(List<ContributionByRouteSubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute
                        && c.SubstanceCode == record.SubstanceCode)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}

using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ContributionBySourceRouteSubstanceSectionBase : ExposureBySourceRouteSubstanceSectionBase {
        public override bool SaveTemporaryData => true;
        public List<ContributionBySourceRouteSubstanceRecord> Records { get; set; }

        protected List<ContributionBySourceRouteSubstanceRecord> summarizeContributions(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRouteSubstanceRecord>();

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (ExposurePath, Substance, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionBySourceRouteSubstanceRecord(
                        ExposurePath,
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

        private static ContributionBySourceRouteSubstanceRecord getContributionBySourceRouteSubstanceRecord(
            ExposurePath path,
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
            var record = new ContributionBySourceRouteSubstanceRecord {
                ExposureSource = path.Source.GetShortDisplayName(),
                ExposureRoute = path.Route.GetShortDisplayName(),
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Contribution = total,
                Percentage = weights.Count / (double)exposures.Count * 100,
                Mean = total / weightsAll.Sum(),
                NumberOfDays = weights.Count,
                Contributions = [],
                UncertaintyLowerBound = uncertaintyLowerBound,
                UncertaintyUpperBound = uncertaintyUpperBound,
                RelativePotencyFactor = rpf ?? double.NaN
            };
            return record;
        }

        protected List<ContributionBySourceRouteSubstanceRecord> SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceRouteSubstanceRecord>();

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (ExposurePath, Substance, Exposures) in exposureCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionBySourceRouteSubstanceRecord {
                        ExposureSource = ExposurePath.Source.GetShortDisplayName(),
                        ExposureRoute = ExposurePath.Route.GetShortDisplayName(),
                        SubstanceName = Substance.Name,
                        SubstanceCode = Substance.Code,
                        Contribution = Exposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight)
                            * relativePotencyFactors?[Substance] ?? double.NaN
                            * membershipProbabilities?[Substance] ?? double.NaN,
                    };
                    result.Add(record);
                }
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. result.OrderByDescending(c => c.Contribution)];
        }

        protected void UpdateContributions(List<ContributionBySourceRouteSubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource 
                    && c.ExposureRoute == record.ExposureRoute
                    && c.SubstanceCode == record.SubstanceCode)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}

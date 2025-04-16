﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class ContributionBySourceSubstanceSectionBase : ExposureBySourceSubstanceSectionBase {
        public override bool SaveTemporaryData => true;
        public List<ContributionBySourceSubstanceRecord> Records { get; set; }

        protected List<ContributionBySourceSubstanceRecord> SummarizeContributions(
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
            var result = new List<ContributionBySourceSubstanceRecord>();
            var exposureSourceSubstanceCollection = CalculateExposures(
                    externalIndividualExposures,
                    substances,
                    kineticConversionFactors,
                    isPerPerson
                );

            foreach (var (Source, Substance, Exposures) in exposureSourceSubstanceCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = getContributionBySourceSubstanceRecord(
                        Source,
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

        private static ContributionBySourceSubstanceRecord getContributionBySourceSubstanceRecord(
            ExposureSource source,
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
                * (rpf.HasValue ? rpf.Value : double.NaN)
                * (membership.HasValue ? membership.Value : double.NaN);
            var record = new ContributionBySourceSubstanceRecord {
                ExposureSource = source.GetShortDisplayName(),
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
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

        protected List<ContributionBySourceSubstanceRecord> SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var result = new List<ContributionBySourceSubstanceRecord>();
            var exposureSourceSubstanceCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            foreach (var (Source, Substance, Exposures) in exposureSourceSubstanceCollection) {
                if (Exposures.Any(c => c.Exposure > 0)) {
                    var record = new ContributionBySourceSubstanceRecord {
                        ExposureSource = Source.GetDisplayName(),
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

        protected void UpdateContributions(List<ContributionBySourceSubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureSource == record.ExposureSource
                        && c.SubstanceCode == record.SubstanceCode)
                    ?.Contribution * 100
                    ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}

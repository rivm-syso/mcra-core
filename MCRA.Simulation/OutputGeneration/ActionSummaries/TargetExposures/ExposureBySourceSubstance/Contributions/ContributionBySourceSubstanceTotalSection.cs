using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySourceSubstanceTotalSection : ContributionBySourceSubstanceSectionBase {

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            relativePotencyFactors = substances.Count > 1
                ? relativePotencyFactors : substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = substances.Count > 1
                ? membershipProbabilities : substances.ToDictionary(r => r, r => 1D);

            Records = SummarizeContributions(
                externalIndividualExposures,
                substances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isPerPerson
            );
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            relativePotencyFactors = substances.Count > 1
                ? relativePotencyFactors : substances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = substances.Count > 1
                ? membershipProbabilities : substances.ToDictionary(r => r, r => 1D);

            var records = summarizeUncertainty(
                 externalIndividualExposures,
                 substances,
                 relativePotencyFactors,
                 membershipProbabilities,
                 kineticConversionFactors,
                 isPerPerson
             );
            UpdateContributions(records);
        }
    }
}

using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalContributionBySourceRouteUpperSection : ExternalContributionBySourceRouteSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);
            UpperPercentage = 100 - percentageForUpperTail;

            var totalExposures = getSumExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            var weights = totalExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var intakeValue = totalExposures.Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var upperExposures = totalExposures
                .Where(c => c.Exposure > intakeValue)
                .Select(c => (
                    c.Exposure,
                    c.SimulatedIndividual
                )).ToList();

            var individualIds = upperExposures.Select(c => c.SimulatedIndividual).ToHashSet();
            var exposures = upperExposures.Select(c => c.Exposure).ToList();
            NumberOfIntakes = upperExposures.Count;
            CalculatedUpperPercentage = upperExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) 
                / totalExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            if (NumberOfIntakes > 0) {
                LowPercentileValue = exposures.Min();
                HighPercentileValue = exposures.Max();
            }

            externalIndividualExposures = externalIndividualExposures.Where(c => individualIds.Contains(c.SimulatedIndividual)).ToList();

            Records = summarizeContributions(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                externalExposureUnit,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isPerPerson
            );
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var totalExposures = getSumExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            var weights = totalExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var intakeValue = totalExposures.Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperExposures = totalExposures
               .Where(c => c.Exposure > intakeValue)
               .Select(c => (
                   c.Exposure,
                   c.SimulatedIndividual
               )).ToList();

            var individualIds = upperExposures.Select(c => c.SimulatedIndividual).ToHashSet();

            externalIndividualExposures = externalIndividualExposures.Where(c => individualIds.Contains(c.SimulatedIndividual)).ToList();

            var records = SummarizeUncertainty(
                  externalIndividualExposures,
                  relativePotencyFactors,
                  membershipProbabilities,
                  isPerPerson
              );
            UpdateContributions(records);
        }

        private static List<(SimulatedIndividual SimulatedIndividual, double Exposure)> getSumExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var exposurePathCollection = CalculateExposures(
                externalIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );
            var totalExposures = exposurePathCollection
                .SelectMany(c => c.Exposures)
                .GroupBy(c => c.SimulatedIndividual)
                .Select(c => (
                    SimulatedIndividual: c.Key,
                    Exposure: c.Sum(r => r.Exposure)
                )).ToList();
            return totalExposures;
        }
    }
}


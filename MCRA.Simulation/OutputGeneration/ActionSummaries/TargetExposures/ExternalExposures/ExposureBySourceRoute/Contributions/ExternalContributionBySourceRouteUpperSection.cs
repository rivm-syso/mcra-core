using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
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
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
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
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            var weights = totalExposures.Select(c => c.SamplingWeight).ToList();
            var intakeValue = totalExposures.Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var upperExposures = totalExposures
                .Where(c => c.Exposure > intakeValue)
                .Select(c => (
                    c.Exposure,
                    c.IndividualId,
                    c.SamplingWeight
                )).ToList();

            var individualIds = upperExposures.Select(c => c.IndividualId).ToHashSet();
            var exposures = upperExposures.Select(c => c.Exposure).ToList();
            NumberOfIntakes = upperExposures.Count;
            CalculatedUpperPercentage = upperExposures.Sum(c => c.SamplingWeight) / totalExposures.Sum(c => c.SamplingWeight) * 100;
            if (NumberOfIntakes > 0) {
                LowPercentileValue = exposures.Min();
                HighPercentileValue = exposures.Max();
            }
            Records = summarizeContributions(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                routes,
                externalExposureUnit,
                individualIds,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isPerPerson
            );
        }


        public void SummarizeUncertainty(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            double percentageForUpperTail,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var totalExposures = getSumExposures(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            var weights = totalExposures.Select(c => c.SamplingWeight).ToList();
            var intakeValue = totalExposures.Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperExposures = totalExposures
               .Where(c => c.Exposure > intakeValue)
               .Select(c => (
                   c.Exposure,
                   c.IndividualId,
                   c.SamplingWeight
               )).ToList();

            var individualIds = upperExposures.Select(c => c.IndividualId).ToHashSet();

            var records = SummarizeUncertainty(
                  externalExposureCollections,
                  observedIndividualMeans,
                  relativePotencyFactors,
                  membershipProbabilities,
                  routes,
                  externalExposureUnit,
                  individualIds,
                  isPerPerson
              );
            UpdateContributions(records);
        }

        private static List<(double Exposure, double SamplingWeight, int IndividualId)> getSumExposures(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var exposures = externalExposureCollections
                .SelectMany(c => c.ExternalIndividualDayExposures
                    .Select(r => (
                        Exposure: r.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson),
                        IndividualId: r.SimulatedIndividual.Individual.Id,
                        SamplingWeight: r.SimulatedIndividual.SamplingWeight
                    ))
                )
                .GroupBy(c => c.IndividualId)
                .Select(c => (
                    Exposure: c.Sum(r => r.Exposure),
                    c.First().SamplingWeight,
                    c.First().IndividualId
                )).ToList();

            if (observedIndividualMeans != null) {
                var oims = observedIndividualMeans.Select(c => (
                    Exposure: c.DietaryIntakePerMassUnit,
                    SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                    c.SimulatedIndividual.Individual.Id
                )).ToList();
                exposures.AddRange(oims);
            }

            var totalExposures = exposures.GroupBy(c => c.IndividualId)
                .Select(c => (
                    Exposure: c.Sum(r => r.Exposure),
                    c.First().SamplingWeight,
                    c.First().IndividualId
                )).ToList();

            return totalExposures;
        }
    }
}


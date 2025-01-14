using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySourceUpperSection : ContributionBySourceSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
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


            var observedIndividualMeans = GetInternalObservedIndividualMeans(
                dietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            var totalExposures = getSumExposures(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                routes,
                isPerPerson
            );

            var weights = totalExposures.Select(c => c.SamplingWeight).ToList();
            var intakeValue = totalExposures.Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);
            var upperExposures = totalExposures
                .Where(c => c.Exposure > intakeValue)
                .Select(c => (
                    c.Exposure,
                    c.SimulatedIndividualId,
                    c.SamplingWeight
                    )
                ).ToList();
            var individualIds = upperExposures.Select(c => c.SimulatedIndividualId).ToHashSet();
            var exposures = upperExposures.Select(c => c.Exposure).ToList();
            NumberOfIntakes = upperExposures.Count;
            CalculatedUpperPercentage = upperExposures.Sum(c => c.SamplingWeight) / totalExposures.Sum(c => c.SamplingWeight) * 100;
            if (NumberOfIntakes > 0) {
                LowPercentileValue = exposures.Min();
                HighPercentileValue = exposures.Max();
            }
            Records = SummarizeContributions(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                routes,
                kineticConversionFactors,
                externalExposureUnit,
                individualIds,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isPerPerson
            );
        }


        public void SummarizeUncertainty(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> routes,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            ExposureUnitTriple externalExposureUnit,
            bool isPerPerson
        ) {
            relativePotencyFactors = activeSubstances.Count > 1
                ? relativePotencyFactors : activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = activeSubstances.Count > 1
                ? membershipProbabilities : activeSubstances.ToDictionary(r => r, r => 1D);

            var observedIndividualMeans = GetInternalObservedIndividualMeans(
                dietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                isPerPerson
            );

            var totalExposures = getSumExposures(
                externalExposureCollections,
                observedIndividualMeans,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                routes,
                isPerPerson
            );

            var weights = totalExposures.Select(c => c.SamplingWeight).ToList();
            var intakeValue = totalExposures.Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperExposures = totalExposures
               .Where(c => c.Exposure > intakeValue)
               .Select(c => (
                   c.Exposure,
                   c.SimulatedIndividualId,
                   c.SamplingWeight
                   )
               ).ToList();
            var individualIds = upperExposures.Select(c => c.SimulatedIndividualId).ToHashSet();

            var records = SummarizeUncertainty(
                  externalExposureCollections,
                  observedIndividualMeans,
                  relativePotencyFactors,
                  membershipProbabilities,
                  routes,
                  kineticConversionFactors,
                  externalExposureUnit,
                  individualIds,
                  isPerPerson
              );
            UpdateContributions(records);
        }

        private static List<(double SamplingWeight, double Exposure, int SimulatedIndividualId)> getSumExposures(
            ICollection<ExternalExposureCollection> externalExposureCollections,
            List<(double SamplingWeight, double Exposure, int SimulatedIndividualId, ExposureSource Source)> observedIndividualMeans,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            ICollection<ExposureRoute> routes,
            bool isPerPerson
        ) {
            var exposures = externalExposureCollections
                .SelectMany(c => c.ExternalIndividualDayExposures
                    .Select(r => (
                        SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                        Exposure: r.GetTotalExternalExposure(
                                relativePotencyFactors,
                                membershipProbabilities,
                                kineticConversionFactors,
                                isPerPerson
                            ),
                        SimulatedIndividualId: r.SimulatedIndividual.Id,
                        Source: c.ExposureSource
                    ))
                ).ToList();

            if (observedIndividualMeans != null) {
                exposures.AddRange(observedIndividualMeans);
            }

            var totalExposures = exposures.GroupBy(c => c.SimulatedIndividualId)
                .Select(c => (
                    SamplingWeight: c.First().SamplingWeight,
                    Exposure: c.Sum(r => r.Exposure),
                    SimulatedIndividualId: c.First().SimulatedIndividualId
                )).ToList();
            return totalExposures;
        }
    }
}


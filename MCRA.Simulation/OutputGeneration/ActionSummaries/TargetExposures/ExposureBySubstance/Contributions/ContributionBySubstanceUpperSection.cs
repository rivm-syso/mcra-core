using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySubstanceUpperSection : ContributionBySubstanceSectionBase {
        public double? UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            if (substances.Count == 1 || relativePotencyFactors != null) {
                UpperPercentage = 100 - percentageForUpperTail;
                var totalExposures = getSumExposures(
                  externalIndividualExposures,
                  substances,
                  relativePotencyFactors,
                  membershipProbabilities,
                  kineticConversionFactors,
                  isPerPerson
              );

                var weights = totalExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                var intakeValue = totalExposures.Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weights, percentageForUpperTail);
                var upperExposures = totalExposures
                    .Where(c => c.Exposure >= intakeValue)
                    .Select(c => (
                        c.Exposure,
                        c.SimulatedIndividual
                        )
                    ).ToList();
                var individualIds = upperExposures.Select(c => c.SimulatedIndividual).ToHashSet();
                var exposures = upperExposures.Select(c => c.Exposure).ToList();
                NumberOfIntakes = upperExposures.Count;
                CalculatedUpperPercentage = upperExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) / totalExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
                if (NumberOfIntakes > 0) {
                    LowPercentileValue = exposures.Min();
                    HighPercentileValue = exposures.Max();
                }

                externalIndividualExposures = externalIndividualExposures.Where(c => individualIds.Contains(c.SimulatedIndividual)).ToList();

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
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            bool isPerPerson
         ) {
            if (substances.Count == 1 || relativePotencyFactors != null) {
                var totalExposures = getSumExposures(
                    externalIndividualExposures,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    isPerPerson
                );
                var weights = totalExposures.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
                var intakeValue = totalExposures.Select(c => c.Exposure)
                    .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

                var upperExposures = totalExposures
                   .Where(c => c.Exposure >= intakeValue)
                   .Select(c => (
                       c.Exposure,
                       c.SimulatedIndividual
                       )
                   ).ToList();
                var individualIds = upperExposures.Select(c => c.SimulatedIndividual).ToHashSet();
                externalIndividualExposures = externalIndividualExposures.Where(c => individualIds.Contains(c.SimulatedIndividual)).ToList();

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
        private static List<(SimulatedIndividual SimulatedIndividual, double Exposure)> getSumExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            var totalExposures = exposureCollection
                .SelectMany(c => c.Exposures, (c, r) => (
                    SimulatedIndividual: r.SimulatedIndividual,
                    Exposures: r.Exposure
                        * (relativePotencyFactors?[c.Substance] ?? double.NaN)
                        * (membershipProbabilities?[c.Substance] ?? double.NaN)
                    ))
                .GroupBy(c => c.SimulatedIndividual)
                .Select(c => (
                    SimulatedIndividual: c.Key,
                    Exposure: c.Sum(r => r.Exposures)
                )).ToList();
            return totalExposures;
        }
    }
}

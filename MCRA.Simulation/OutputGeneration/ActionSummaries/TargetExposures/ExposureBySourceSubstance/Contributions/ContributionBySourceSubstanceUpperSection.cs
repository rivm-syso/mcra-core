﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ContributionBySourceSubstanceUpperSection : ContributionBySourceSubstanceSectionBase {
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

            var totalExposures = getSumExposures(
                externalIndividualExposures,
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
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
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
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
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
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
                activeSubstances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
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
                   )
               ).ToList();
            var individualIds = upperExposures.Select(c => c.SimulatedIndividual).ToHashSet();
            externalIndividualExposures = externalIndividualExposures.Where(c => individualIds.Contains(c.SimulatedIndividual)).ToList();

            var records = SummarizeUncertainty(
                  externalIndividualExposures,
                  activeSubstances,
                  relativePotencyFactors,
                  membershipProbabilities,
                  kineticConversionFactors,
                  isPerPerson
              );
            UpdateContributions(records);
        }

        private static List<(SimulatedIndividual SimulatedIndividual, double Exposure)> getSumExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var exposureSourceSubstanceCollection = CalculateExposures(
                externalIndividualExposures,
                substances,
                kineticConversionFactors,
                isPerPerson
            );

            var totalExposures = exposureSourceSubstanceCollection
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

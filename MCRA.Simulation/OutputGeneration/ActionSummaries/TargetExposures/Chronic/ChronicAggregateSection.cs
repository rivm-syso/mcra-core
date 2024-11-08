﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public class ChronicAggregateSection : ChronicSectionBase {

        public void Summarize(
            SectionHeader header,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit,
            Compound referenceSubstance,
            ExposureMethod exposureMethod,
            double[] selectedExposureLevels,
            double[] selectedPercentiles,
            double percentageForUpperTail,
            bool isAggregate
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors
                    ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities
                    ?? substances.ToDictionary(r => r, r => 1D);
            }

            var exposures = aggregateIndividualExposures
                .Select(c => c
                    .GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    )
                )
                .ToList();
            var weights = aggregateIndividualExposures
                .Select(c => c.IndividualSamplingWeight)
                .ToList();

            // Total distribution section
            var totalDistributionSection = new OIMDistributionSection(true, isAggregate);
            var subHeader = header.AddSubSectionHeaderFor(totalDistributionSection, "Graph total", 1);
            totalDistributionSection
                .Summarize(exposures, weights);
            totalDistributionSection
                .SummarizeCategorizedBins(
                    aggregateIndividualExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureRoutes,
                    kineticConversionFactors,
                    externalExposureUnit,
                    targetUnit
                );
            subHeader.SaveSummarySection(totalDistributionSection);

            // Upper distribution section
            var upperDistributionSection = new OIMDistributionSection(false, true);
            subHeader = header.AddSubSectionHeaderFor(upperDistributionSection, "Graph upper tail", 2);
            upperDistributionSection
                .SummarizeUpperAggregate(
                    aggregateIndividualExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetUnit,
                    percentageForUpperTail
                );
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                exposures,
                exposureMethod,
                selectedExposureLevels
            );
            subHeader.SaveSummarySection(upperDistributionSection);

            // Exposure percentile section
            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 3);
            percentileSection.Summarize(exposures, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            // Exposure percentages section
            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 4);
            percentageSection.Summarize(exposures, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes
        /// 1) Percentiles
        /// 2) Percentages
        /// 3) Percentiles for a grid of 100 values
        /// </summary>
        public void SummarizeUncertainty(
            SectionHeader header,
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors
                    ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities
                    ?? substances.ToDictionary(r => r, r => 1D);
            }

            var aggregateIntakes = aggregateExposures
                .Select(c => c
                    .GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    )
                )
                .ToList();
            var weights = aggregateExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(aggregateIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(aggregateIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<OIMDistributionSection>();
            if (subHeader != null) {
                var totalDistributionSection = subHeader.GetSummarySection() as OIMDistributionSection;
                totalDistributionSection.SummarizeUncertainty(aggregateIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}

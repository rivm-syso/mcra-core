using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class AggregateIntakeDistributionSection : SummarySection {

        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes plots untransformed, total, upper, percentiles, percentages.
        /// </summary>
        public void Summarize(
            SectionHeader header,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
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
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            if (activeSubstances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors ?? activeSubstances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities ?? activeSubstances.ToDictionary(r => r, r => 1D);
            }
            var exposures = aggregateIndividualDayExposures
                .Select(c => c.GetTotalExposureAtTarget(
                    targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities
                ))
                .ToList();
            var exposureLevels = ExposureLevelsCalculator
                .GetExposureLevels(
                    exposures,
                    exposureMethod,
                    selectedExposureLevels
                );

            var coExposureIntakes = aggregateIndividualDayExposures
                .AsParallel()
                .Where(idi => idi.IsCoExposure())
                .Select(c => c.SimulatedIndividualDayId)
                .ToList();
            var untransformedTotalIntakeDistributionSection = new UntransformedTotalIntakeDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(untransformedTotalIntakeDistributionSection, "Graph untransformed", 1);
            untransformedTotalIntakeDistributionSection.Summarize(
                aggregateIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                targetUnit.Target
            );
            subHeader.SaveSummarySection(untransformedTotalIntakeDistributionSection);

            var totalIntakeDistributionSection = new AggregateTotalIntakeDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(totalIntakeDistributionSection, "Graph total", 2);
            totalIntakeDistributionSection.Summarize(
                coExposureIntakes,
                aggregateIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                exposureRoutes,
                externalExposureUnit,
                targetUnit,
                GriddingFunctions.GetPlotPercentages(),
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
            subHeader.SaveSummarySection(totalIntakeDistributionSection);

            var upperIntakeDistributionSection = new AggregateUpperIntakeDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(upperIntakeDistributionSection, "Graph upper tail", 3);
            upperIntakeDistributionSection.Summarize(
                coExposureIntakes,
                aggregateIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                exposureRoutes,
                externalExposureUnit,
                targetUnit,
                percentageForUpperTail,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
            subHeader.SaveSummarySection(upperIntakeDistributionSection);

            var aggregateIntakes = aggregateIndividualDayExposures
                .Select(c => c
                    .GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    )
                )
                .ToList();
            var weights = aggregateIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 4);
            percentileSection.Summarize(aggregateIntakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 5);
            percentageSection.Summarize(aggregateIntakes, weights, referenceSubstance, exposureLevels);
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

            subHeader = header.GetSubSectionHeader<AggregateTotalIntakeDistributionSection>();
            if (subHeader != null) {
                var totalIntakeDistributionSection = subHeader.GetSummarySection() as AggregateTotalIntakeDistributionSection;
                totalIntakeDistributionSection.SummarizeUncertainty(
                    aggregateExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    targetUnit.Target
                );
            }
        }
    }
}

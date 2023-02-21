using MCRA.Utils;
using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class AggregateIntakeDistributionSection : SummarySection {

        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes plots untransformed, total, upper, percentiles, percentages.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="absorptionFactors"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedExposureLevels"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="uncertaintyLowerLimit"></param>
        /// <param name="uncertaintyUpperLimit"></param>
        public void Summarize(
            SectionHeader header,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<ExposureRouteType> exposureRoutes,
            Compound referenceSubstance,
            ExposureMethod exposureMethod,
            double[] selectedExposureLevels,
            double[] selectedPercentiles,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            if (activeSubstances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors ?? activeSubstances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities ?? activeSubstances.ToDictionary(r => r, r => 1D);
            }
            var intakes = aggregateIndividualDayExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                intakes,
                exposureMethod,
                selectedExposureLevels);

            var coExposureIntakes = getCoExposures(aggregateIndividualDayExposures);
            var untransformedTotalIntakeDistributionSection = new UntransformedTotalIntakeDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(untransformedTotalIntakeDistributionSection, "Graph untransformed", 1);
            untransformedTotalIntakeDistributionSection.Summarize(
                aggregateIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );
            subHeader.SaveSummarySection(untransformedTotalIntakeDistributionSection);

            var totalIntakeDistributionSection = new AggregateTotalIntakeDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(totalIntakeDistributionSection, "Graph total", 2);
            totalIntakeDistributionSection.Summarize(
                coExposureIntakes,
                aggregateIndividualDayExposures,
                relativePotencyFactors,
                membershipProbabilities,
                absorptionFactors,
                exposureRoutes, GriddingFunctions.GetPlotPercentages(),
                isPerPerson,
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
                absorptionFactors,
                exposureRoutes,
                percentageForUpperTail,
                isPerPerson,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
            subHeader.SaveSummarySection(upperIntakeDistributionSection);

            var aggregateIntakes = aggregateIndividualDayExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
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
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            SectionHeader header,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            if (activeSubstances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors ?? activeSubstances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities ?? activeSubstances.ToDictionary(r => r, r => 1D);
            }

            var aggregateIntakes = aggregateIndividualDayExposures.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = aggregateIndividualDayExposures.Select(c => c.IndividualSamplingWeight).ToList();
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
                totalIntakeDistributionSection.SummarizeUncertainty(aggregateIndividualDayExposures, relativePotencyFactors, membershipProbabilities, isPerPerson);
            }
        }

        private List<int> getCoExposures(ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            return aggregateIndividualDayExposures
                .AsParallel()
                .WithCancellation(cancelToken)
                .Where(idi => idi.IsCoExposure())
                .Select(c => c.SimulatedIndividualDayId)
                .ToList();
        }
    }
}

using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class ChronicAggregateSection : ChronicSectionBase {

        /// <summary>
        /// /// In general exposure is expressed per microgr/kg bw/day
        /// !!============ For DIETARY EXPOSURE ============!!
        /// Suppose that the p50 of DIETARY exposure = 8.13 microgr/kg bw/day. When exposure is expressed per person and the average bodyweight = 10 kg
        /// then the exposure expressed per person = 81.3 microgr/kg bw/day (10 * 8.13). For a person with bodyweight 100 kg it will be 813 microgr/kg bw/day.
        /// 
        /// !!============ For TARGET EXPOSURE ============!!
        /// For target exposure at the liver, the relative compartment weight = 0.024.
        /// Suppose that the p50 of TARGET exposure = 8.13 microgr/kg. When exposure is expressed per person and the average bodyweight = 10 kg
        /// then the exposure expressed per person = 1.95 microgr/kg day(10 * 0.024 * 8.13). For a  person with bodyweight 100 kg it will be 19.51 microgr/kg day(100 * 0.024 * 8.13).
        /// Especially for the bw = 10 kg this seems contra-intuitive, but the liver of a person of 10 kg weighs 0.24 kg and for a person of 100 kg it weighs 2.4 kg.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="aggregateIndividualMeans"></param>
        /// <param name="substances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="absorptionFactors"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedExposureLevels"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isAggregate"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            SectionHeader header,
            ICollection<AggregateIndividualExposure> aggregateIndividualMeans,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRouteType, Compound), double> absorptionFactors,
            ICollection<ExposureRouteType> exposureRoutes,
            Compound referenceSubstance,
            ExposureMethod exposureMethod,
            double[] selectedExposureLevels,
            double[] selectedPercentiles,
            double percentageForUpperTail,
            bool isAggregate,
            bool isPerPerson
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);
            }

            var intakes = aggregateIndividualMeans.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = aggregateIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();

            // Total distribution section
            var totalDistributionSection = new OIMDistributionSection(true, isAggregate);
            var subHeader = header.AddSubSectionHeaderFor(totalDistributionSection, "Graph total", 1);
            totalDistributionSection.Summarize(intakes, weights);
            totalDistributionSection.SummarizeCategorizedBins(aggregateIndividualMeans, relativePotencyFactors, membershipProbabilities, exposureRoutes, absorptionFactors, isPerPerson);
            subHeader.SaveSummarySection(totalDistributionSection);

            // Upper distribution section
            var upperDistributionSection = new OIMDistributionSection(false, true);
            subHeader = header.AddSubSectionHeaderFor(upperDistributionSection, "Graph upper tail", 2);
            upperDistributionSection.SummarizeUpperAggregate(aggregateIndividualMeans, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                intakes,
                exposureMethod,
                selectedExposureLevels
            );
            subHeader.SaveSummarySection(upperDistributionSection);

            // Exposure percentile section
            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 3);
            percentileSection.Summarize(intakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            // Exposure percentages section
            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 4);
            percentageSection.Summarize(intakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarize uncertainty
        /// </summary>
        /// <param name="header"></param>
        /// <param name="aggregateIntakes"></param>
        /// <param name="substances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            SectionHeader header,
            ICollection<AggregateIndividualExposure> aggregateIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities ?? substances.ToDictionary(r => r, r => 1D);
            }

            var intakes = aggregateIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = aggregateIntakes.Select(c => c.IndividualSamplingWeight).ToList();

            var subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<OIMDistributionSection>();
            if (subHeader != null) {
                var totalDistributionSection = subHeader.GetSummarySection() as OIMDistributionSection;
                totalDistributionSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}

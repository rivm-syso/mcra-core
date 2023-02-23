using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ModelAssistedSection : ChronicSectionBase {

        /// <summary>
        /// Summarizes dietary individual intakes chronic model assisted for BBN, LNN0, LNN.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryModelAssistedIntakes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="selectedExposureLevels"></param>
        /// <param name="upperPercentage"></param>
        public void Summarize(
            SectionHeader header,
            List<DietaryIndividualIntake> dietaryModelAssistedIntakes,
            Compound referenceSubstance,
            ExposureMethod exposureMethod,
            double[] selectedPercentiles,
            double[] selectedExposureLevels,
            double upperPercentage
        ) {
            var intakes = dietaryModelAssistedIntakes.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = dietaryModelAssistedIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                intakes,
                exposureMethod,
                selectedExposureLevels
            );
            var totalDistributionSection = new ModelAssistedDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(totalDistributionSection, "Graph total", 10);
            totalDistributionSection.Summarize(intakes, weights);
            subHeader.SaveSummarySection(totalDistributionSection);

            var upperDistributionSection = new ModelAssistedDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(upperDistributionSection, "Graph upper tail", 11);
            upperDistributionSection.SummarizeUpperDietary(dietaryModelAssistedIntakes, upperPercentage);
            subHeader.SaveSummarySection(upperDistributionSection);

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 20);
            percentileSection.Summarize(intakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 30);
            percentageSection.Summarize(intakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes aggregate individual intakes chronic for BBN, LNN0, LNN.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="aggregateIndividualExposures"></param>
        /// <param name="substances"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="selectedExposureLevels"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            SectionHeader header,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            Compound referenceSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureMethod exposureMethod,
            double[] selectedPercentiles,
            double[] selectedExposureLevels,
            double upperPercentage,
            bool isPerPerson
        ) {
            var rpfs = relativePotencyFactors ?? substances?.ToDictionary(r => r, r => 1D);
            var memberships = membershipProbabilities ?? substances?.ToDictionary(r => r, r => 1D);

            var intakes = aggregateIndividualExposures.Select(c => c.TotalConcentrationAtTarget(rpfs, memberships, isPerPerson)).ToList();
            var weights = aggregateIndividualExposures.Select(c => c.IndividualSamplingWeight).ToList();
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                intakes,
                exposureMethod,
                selectedExposureLevels
            );
            var totalDistributionSection = new ModelAssistedDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(totalDistributionSection, "Graph total", 10);
            totalDistributionSection.Summarize(intakes, weights);
            subHeader.SaveSummarySection(totalDistributionSection);

            var upperDistributionSection = new ModelAssistedDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(upperDistributionSection, "Graph upper tail", 11);
            upperDistributionSection.SummarizeUpperAggregate(
                aggregateIndividualExposures,
                rpfs,
                memberships, 
                upperPercentage, 
                isPerPerson
            );
            subHeader.SaveSummarySection(upperDistributionSection);

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 20);
            percentileSection.Summarize(intakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 30);
            percentageSection.Summarize(intakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes the uncertainty results aggregate.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="aggregateModelAssistedIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            SectionHeader header,
            List<AggregateIndividualExposure> aggregateModelAssistedIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var intakes = aggregateModelAssistedIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = aggregateModelAssistedIntakes.Select(c => c.IndividualSamplingWeight).ToList();

            var subHeader = header.GetSubSectionHeader<ModelAssistedDistributionSection>();
            if (subHeader != null) {
                var intakeDistributionSection = subHeader.GetSummarySection() as ModelAssistedDistributionSection;
                intakeDistributionSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }

        /// <summary>
        /// Summarizes the uncertainty results model assisted.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="modelAssistedIntakes"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                SectionHeader header,
                List<DietaryIndividualIntake> modelAssistedIntakes,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            var usualIntakes = modelAssistedIntakes.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = modelAssistedIntakes.Select(c => c.IndividualSamplingWeight).ToList();

            var subHeader = header.GetSubSectionHeader<ModelAssistedDistributionSection>();
            if (subHeader != null) {
                var intakeDistributionSection = subHeader.GetSummarySection() as ModelAssistedDistributionSection;
                intakeDistributionSection.SummarizeUncertainty(usualIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(usualIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(usualIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}

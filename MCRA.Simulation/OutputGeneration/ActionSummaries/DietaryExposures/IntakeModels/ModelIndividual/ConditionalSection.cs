using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ConditionalSection : SummarySection {

        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarizes reference results BBN, LNN0.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="cofactor"></param>
        /// <param name="covariable"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="conditionalIntakes"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedExposureLevels"></param>
        ///
        public void Summarize(
                SectionHeader header,
                IndividualProperty cofactor,
                IndividualProperty covariable,
                Compound referenceSubstance,
                List<ConditionalUsualIntake> conditionalIntakes,
                double[] selectedPercentiles,
                ExposureMethod exposureMethod,
                double[] selectedExposureLevels
            ) {
            var percentileSection = new ConditionalIntakePercentileSection();
            var subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 1);
            percentileSection.Summarize(
                cofactor,
                covariable,
                referenceSubstance,
                conditionalIntakes,
                selectedPercentiles
            );
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new ConditionalIntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 2);
            percentageSection.Summarize(
                cofactor,
                covariable,
                referenceSubstance,
                conditionalIntakes,
                exposureMethod,
                selectedExposureLevels
           );
           subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes uncertainty results BBN, LNN0.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="conditionalIntakes"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                SectionHeader header,
                List<ConditionalUsualIntake> conditionalIntakes,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            var subHeader = header.GetSubSectionHeader<ConditionalIntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as ConditionalIntakePercentileSection;
                percentileSection.SummarizeUncertainty(conditionalIntakes, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<ConditionalIntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as ConditionalIntakePercentageSection;
                percentageSection.SummarizeUncertainty(conditionalIntakes, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}

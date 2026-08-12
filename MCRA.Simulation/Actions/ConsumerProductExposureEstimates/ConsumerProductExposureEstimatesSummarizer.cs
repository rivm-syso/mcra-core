using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductExposureEstimates {
    public enum ConsumerProductExposureEstimatesSections {
        ConsumerProductExposureAssessmentSection,
        ConsumerProductExposureSetSection
    }

    public sealed class ConsumerProductExposureEstimatesSummarizer(ConsumerProductExposureEstimatesModuleConfig config)
        : ActionModuleResultsSummarizer<ConsumerProductExposureEstimatesModuleConfig, IConsumerProductExposureEstimatesActionResult>(config) {

        public override ActionType ActionType => ActionType.ConsumerProductExposureEstimates;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IConsumerProductExposureEstimatesActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductExposureEstimatesSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            subHeader.Units = collectUnits(data, sectionConfig);

            if (outputSettings.ShouldSummarize(ConsumerProductExposureEstimatesSections.ConsumerProductExposureAssessmentSection)) {
                summarizeExposureAssessments(
                    data.ConsumerProductExposureAssessments,
                    subHeader,
                    subOrder++
                );
            }
            if (outputSettings.ShouldSummarize(ConsumerProductExposureEstimatesSections.ConsumerProductExposureSetSection)) {
                summarizeExposureSets(
                    data.ConsumerProductExposureAssessments,
                    subHeader,
                    subOrder++
                );
            }
        }
        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}"),
            };
            return result;
        }

        private void summarizeExposureAssessments(
           IDictionary<string, ConsumerProductExposureAssessment> consumerProductExposureAssessments,
           SectionHeader header,
           int order
        ) {
            var section = new ConsumerProductExposureAssessmentSummarySection() {
                SectionLabel = getSectionLabel(ConsumerProductExposureEstimatesSections.ConsumerProductExposureAssessmentSection)
            };
            section.Summarize(
                consumerProductExposureAssessments.Values
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumer product exposure assessments",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeExposureSets(
           IDictionary<string, ConsumerProductExposureAssessment> consumerProductExposureAssessments,
           SectionHeader header,
           int order
        ) {
            var section = new ConsumerProductExposureSetSummarySection() {
                SectionLabel = getSectionLabel(ConsumerProductExposureEstimatesSections.ConsumerProductExposureSetSection)
            };
            section.Summarize(
                consumerProductExposureAssessments.Values
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumer product exposure sets",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}

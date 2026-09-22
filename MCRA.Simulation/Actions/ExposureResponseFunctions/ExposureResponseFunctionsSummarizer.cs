using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ExposureResponseFunctions {
    public enum ExposureResponseFunctionsSections {
        ErfSummarySection,
        ErfSubgroupSummarySection
    }
    public sealed class ExposureResponseFunctionsSummarizer : ActionResultsSummarizerBase<IExposureResponseFunctionsActionResult> {

        public override ActionType ActionType => ActionType.ExposureResponseFunctions;

        public override void Summarize(ActionModuleConfig sectionConfig, IExposureResponseFunctionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ExposureResponseFunctionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subOrder = 0;

            var section = new ErfDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), subOrder++);
            section.Summarize(data.ExposureResponseFunctionModels);
            subHeader.SaveSummarySection(section);

            // Table of ERF subgroups.
            if (outputSettings.ShouldSummarize(ExposureResponseFunctionsSections.ErfSubgroupSummarySection)
                && data.ExposureResponseFunctionModels.Count > 0
            ) {
                summarizeErfSubgroups(
                    data.ExposureResponseFunctionModels,
                    subHeader,
                    subOrder++
                );
            }
        }

        private void summarizeErfSubgroups(
            ICollection<IExposureResponseModel> exposureResponseFunctionModels,
            SectionHeader header,
            int order
        ) {
            var section = new ErfDataSubgroupSummarySection() {
                SectionLabel = getSectionLabel(ExposureResponseFunctionsSections.ErfSubgroupSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposure response function subgroups",
                order
            );
            section.Summarize(exposureResponseFunctionModels);
            subHeader.SaveSummarySection(section);
        }
    }
}

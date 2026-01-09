using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.OccupationalExposures {
    public enum OccupationalExposuresSections {
        OccupationalScenarioExposuresSection,
        OccupationalIndividualExposuresSection,
        OccupationalTaskExposuresSection,
        OccupationalTaskExposureModelsSection
    }

    public sealed class OccupationalExposuresSummarizer(OccupationalExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<OccupationalExposuresModuleConfig, OccupationalExposuresActionResult>(config) {

        public override ActionType ActionType => ActionType.OccupationalExposures;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            OccupationalExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<OccupationalExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            subHeader.Units = collectUnits(result);

            var subOrder = 0;
            if (outputSettings.ShouldSummarize(OccupationalExposuresSections.OccupationalScenarioExposuresSection)
                && (result.OccupationalScenarioExposures?.Count > 0)
            ) {
                summarizeOccupationalScenarioExposures(
                    result,
                    data,
                    subHeader,
                    subOrder++
                 );
            }

            if (outputSettings.ShouldSummarize(OccupationalExposuresSections.OccupationalIndividualExposuresSection)
                && (result.ExternalIndividualExposureCollection != null)
            ) {
                summarizeOccupationalIndividualExposures(
                    result,
                    data,
                    subHeader,
                    subOrder++
                 );
            }

            if (outputSettings.ShouldSummarize(OccupationalExposuresSections.OccupationalTaskExposuresSection)
                && (result.OccupationalScenarioTaskExposures?.Count > 0)
            ) {
                summarizeScenarioTaskExposures(
                    result,
                    subHeader,
                    subOrder++
                 );
            }

            if (outputSettings.ShouldSummarize(OccupationalExposuresSections.OccupationalTaskExposureModelsSection)
                && (result.OccupationalTaskExposureModels?.Count > 0)
            ) {
                summarizeTaskExposureModels(
                    result,
                    subHeader,
                    subOrder++
                 );
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(OccupationalExposuresActionResult actionResult) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("ExposureUnit", actionResult.ExternalSystemicExposureUnit?.GetDisplayName()),
            };
            return result;
        }

        private void summarizeOccupationalIndividualExposures(
            OccupationalExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new OccupationalIndividualExposuresPerRouteSection() {
                SectionLabel = getSectionLabel(OccupationalExposuresSections.OccupationalScenarioExposuresSection),
                Units = header.Units
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Occupational individual exposures",
                order
            );
            section.Summarize(
                result.ExternalIndividualExposureCollection,
                data.ActiveSubstances,
                _configuration.SelectedExposureRoutes,
                result.ExternalSystemicExposureUnit,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );

            subHeader.SaveSummarySection(section);
        }

        private void summarizeOccupationalScenarioExposures(
            OccupationalExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new OccupationalScenarioExposuresSection() {
                SectionLabel = getSectionLabel(OccupationalExposuresSections.OccupationalScenarioExposuresSection),
                Units = header.Units
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Occupational scenario exposures",
                order
            );
            section.Summarize(
                result.OccupationalScenarioExposures,
                data.ActiveSubstances,
                _configuration.SelectedExposureRoutes,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );

            subHeader.SaveSummarySection(section);
        }

        private void summarizeScenarioTaskExposures(
            OccupationalExposuresActionResult result,
            SectionHeader header,
            int order
        ) {
            var section = new OccupationalScenarioTasksExposuresSection() {
                SectionLabel = getSectionLabel(OccupationalExposuresSections.OccupationalTaskExposuresSection),
                Units = header.Units
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Scenario task exposures",
                order
            );
            section.Summarize(result.OccupationalScenarioTaskExposures);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeTaskExposureModels(
            OccupationalExposuresActionResult result,
            SectionHeader header,
            int order
        ) {
            var section = new OccupationalTaskExposureModelsSection() {
                SectionLabel = getSectionLabel(OccupationalExposuresSections.OccupationalTaskExposureModelsSection),
                Units = header.Units
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Occupational task exposure models",
                order
            );
            section.Summarize(result.OccupationalTaskExposureModels);
            subHeader.SaveSummarySection(section);
        }
    }
}
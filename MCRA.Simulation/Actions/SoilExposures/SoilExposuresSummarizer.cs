using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Calculators.SoilExposureCalculation;

namespace MCRA.Simulation.Actions.SoilExposures {
    public enum SoilExposuresSections {
        SoilExposuresByRouteSection
    }
    public sealed class SoilExposuresSummarizer(SoilExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<SoilExposuresModuleConfig, SoilExposuresActionResult>(config) {
        public override ActionType ActionType => ActionType.SoilExposures;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            SoilExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<SoilExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            // Main summary section
            var section = new SoilExposuresSection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(data.IndividualSoilExposures);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by exposure route contributions, summary and boxplot
            summarizeSoilExposuresByRoute(
                actionResult,
                data,
                subHeader,
                order++
            );
        }

        public void SummarizeUncertain(
            SoilExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<SoilExposuresSection>();
            if (subHeader != null) {
                summarizeSoilExposureUncertainty(
                    data.IndividualSoilExposures,
                    header
                );
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
                new("ExposureUnit", data.SoilExposureUnit.GetShortDisplayName()),
            };
            return result;
        }

        private void summarizeSoilExposureUncertainty(
            ICollection<SoilIndividualDayExposure> individualSoilExposures,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<SoilExposuresSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as SoilExposuresSection;
                section.SummarizeUncertainty(
                    individualSoilExposures,
                   _configuration.UncertaintyLowerBound,
                   _configuration.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Soil exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeSoilExposuresByRoute(
            SoilExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new SoilExposuresByRouteSection() {
                SectionLabel = getSectionLabel(SoilExposuresSections.SoilExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route and substance",
                order
            );
            section.Summarize(
                data.ActiveSubstances,
                actionResult.IndividualSoilExposures,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                actionResult.SoilExposureUnit,
                new List<ExposureRoute>([ExposureRoute.Oral])
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
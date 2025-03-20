using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.AirExposures {
    public enum AirExposuresSections {
        AirExposuresByRouteSection
    }

    public sealed class AirExposuresSummarizer(AirExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<AirExposuresModuleConfig, AirExposuresActionResult>(config)
    {
        public override ActionType ActionType => ActionType.AirExposures;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            AirExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<AirExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            // Main summary section
            var section = new AirExposuresSection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(data.IndividualAirExposures);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by exposure route contributions, summary and boxplot
            summarizeAirExposuresByRoute(
                actionResult,
                data,
                subHeader,
                order++
            );
        }

        public void SummarizeUncertain(
            AirExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<AirExposuresSection>();
            if (subHeader != null) {
                summarizeAirExposureUncertainty(
                    data.IndividualAirExposures,
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
                new("AirExposureUnit", data.AirExposureUnit.GetShortDisplayName()),
            };
            return result;
        }

        private void summarizeAirExposureUncertainty(
            ICollection<AirIndividualDayExposure> individualAirExposures,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<AirExposuresSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as AirExposuresSection;
                section.SummarizeUncertainty(
                    individualAirExposures,
                   _configuration.UncertaintyLowerBound,
                   _configuration.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Air exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeAirExposuresByRoute(
            AirExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new AirExposuresByRouteSubstanceSection() {
                SectionLabel = getSectionLabel(AirExposuresSections.AirExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route by substance",
                order
            );
            section.Summarize(
                data.ActiveSubstances,
                actionResult.IndividualAirExposures,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                actionResult.AirExposureUnit,
                _configuration.SelectedExposureRoutes
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
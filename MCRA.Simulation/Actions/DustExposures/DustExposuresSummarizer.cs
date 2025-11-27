using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Actions.DustExposures {
    public enum DustExposuresSections {
        DustExposuresByRouteSection
    }
    public sealed class DustExposuresSummarizer(DustExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<DustExposuresModuleConfig, DustExposuresActionResult>(config)
    {
        public override ActionType ActionType => ActionType.DustExposures;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            DustExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<DustExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            // Main summary section
            var section = new DustExposuresSection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(data.IndividualDustExposures);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by exposure route contributions, summary and boxplot
            summarizeDustExposuresByRoute(
                actionResult,
                data,
                subHeader,
                order++
            );
        }

        public void SummarizeUncertain(
            DustExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<DustExposuresSection>();
            if (subHeader != null) {
                summarizeDustExposureUncertainty(
                    data.IndividualDustExposures,
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
                new("ExposureUnit", data.DustExposureUnit.GetShortDisplayName()),
            };
            return result;
        }

        private void summarizeDustExposureUncertainty(
            ICollection<DustIndividualExposure> individualDustExposures,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<DustExposuresSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as DustExposuresSection;
                section.SummarizeUncertainty(
                    individualDustExposures,
                   _configuration.UncertaintyLowerBound,
                   _configuration.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Dust exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeDustExposuresByRoute(
            DustExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustExposuresByRouteSection() {
                SectionLabel = getSectionLabel(DustExposuresSections.DustExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route and substance",
                order
            );
            section.Summarize(
                data.ActiveSubstances,
                actionResult.IndividualDustExposures,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                actionResult.DustExposureUnit,
                _configuration.SelectedExposureRoutes
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
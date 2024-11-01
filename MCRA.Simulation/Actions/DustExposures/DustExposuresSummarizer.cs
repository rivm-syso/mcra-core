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
    public sealed class DustExposuresSummarizer : ActionModuleResultsSummarizer<DustExposuresModuleConfig, DustExposuresActionResult> {

        public DustExposuresSummarizer(DustExposuresModuleConfig config) : base(config) {
        }

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
            var section = new DustExposuresSection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(
                data.IndividualDustExposures,
                data.ActiveSubstances
            );
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // exposures by exposure route contributions, summary and boxplot
            summarizeDustExposuresByRoute(
                actionResult,
                data,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
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
            if (subHeader == null) {
                return;
            }
            summarizeDustExposureUncertainty(
                data.IndividualDustExposures,
                header
            );
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}")
            };
            return result;
        }

        private void summarizeDustExposureUncertainty(
            ICollection<DustIndividualDayExposure> individualDustExposures,
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
        /// Boxplot and summary table
        /// </summary>
        private void summarizeDustExposuresByRoute(
            DustExposuresActionResult actionResult,
            ActionData data,
            double lowerPercentage,
            double upperPercentage,
            SectionHeader header,
            int order
        ) {
            var section = new DustExposuresByRouteSection() {
                SectionLabel = getSectionLabel(DustExposuresSections.DustExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route by substance",
                order
            );
            section.Summarize(
                data.AllCompounds,
                actionResult.IndividualDustExposures,
                lowerPercentage,
                upperPercentage,
                actionResult.DustExposureUnit
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
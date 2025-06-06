using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductExposures {
    public enum ConsumerProductExposuresSections {
        ConsumerProductExposuresByRouteSection
    }
    public sealed class ConsumerProductExposuresSummarizer(ConsumerProductExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<ConsumerProductExposuresModuleConfig, ConsumerProductExposuresActionResult>(config)
    {
        public override ActionType ActionType => ActionType.ConsumerProductExposures;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            ConsumerProductExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductExposuresSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            // Main summary section
            var section = new ConsumerProductExposuresSection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(data.IndividualConsumerProductExposures);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by exposure route contributions, summary and boxplot
            summarizeConsumerProductExposuresByRoute(
                actionResult,
                data,
                subHeader,
                order++
            );
        }

        public void SummarizeUncertain(
            ConsumerProductExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<DustExposuresSection>();
            //if (subHeader != null) {
            //    summarizeDustExposureUncertainty(
            //        data.IndividualDustExposures,
            //        header
            //    );
            //}
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
                new("ExposureUnit", data.ConsumerProductExposureUnit.GetShortDisplayName()),
            };
            return result;
        }

        //private void summarizeDustExposureUncertainty(
        //    ICollection<DustIndividualDayExposure> individualDustExposures,
        //    SectionHeader header
        //) {
        //    var subHeader = header.GetSubSectionHeader<DustExposuresSection>();
        //    if (subHeader != null) {
        //        var section = subHeader.GetSummarySection() as DustExposuresSection;
        //        section.SummarizeUncertainty(
        //            individualDustExposures,
        //           _configuration.UncertaintyLowerBound,
        //           _configuration.UncertaintyUpperBound
        //        );
        //        subHeader.SaveSummarySection(section);
        //    }
        //}

        /// <summary>
        /// Dust exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeConsumerProductExposuresByRoute(
            ConsumerProductExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ConsumerProductExposuresByRouteSection() {
                SectionLabel = getSectionLabel(ConsumerProductExposuresSections.ConsumerProductExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route by substance",
                order
            );
            section.Summarize(
                data.ActiveSubstances,
                actionResult.ConsumerProductIndividualExposures,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                actionResult.ConsumerProductExposureUnit,
                new List<ExposureRoute>() { ExposureRoute.Dermal}
                //_configuration.SelectedExposureRoutes
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductExposures {
    public enum ConsumerProductExposuresSections {
        ConsumerProductExposuresByRouteSubstanceSection,
        ConsumerProductExposuresByRouteSection,
        ConsumerProductsSection
    }

    public sealed class ConsumerProductExposuresSummarizer(ConsumerProductExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<ConsumerProductExposuresModuleConfig, ConsumerProductExposuresActionResult>(config) {
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
            section.Summarize(actionResult.ConsumerProductIndividualIntakes);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by exposure route and substance: contributions, summary and boxplot
            if (outputSettings.ShouldSummarize(ConsumerProductExposuresSections.ConsumerProductExposuresByRouteSubstanceSection)) {
                summarizeConsumerProductExposuresByRouteSubstance(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }

            // Exposures by route: contributions, summary and boxplot
            if (data.ActiveSubstances.Count >  0
                && outputSettings.ShouldSummarize(ConsumerProductExposuresSections.ConsumerProductExposuresByRouteSection)
            ) {
                summarizeConsumerProductExposuresByRoute(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }

            // Exposures by consumer products: contributions, summary and boxplot
            if (outputSettings.ShouldSummarize(ConsumerProductExposuresSections.ConsumerProductsSection)) {
                summarizeConsumerProducts(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }
        }

        public void SummarizeUncertain(
            ConsumerProductExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            // TODO
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

        /// <summary>
        /// Consumer product exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeConsumerProductExposuresByRouteSubstance(
            ConsumerProductExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ConsumerProductExposuresByRouteSubstanceSection() {
                SectionLabel = getSectionLabel(ConsumerProductExposuresSections.ConsumerProductExposuresByRouteSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route and substance",
                order
            );
            section.Summarize(
                data.ActiveSubstances,
                result.ConsumerProductIndividualIntakes,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                result.ConsumerProductExposureUnit,
                _configuration.SelectedExposureRoutes
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Consumer product exposures by route (boxplot and summary table).
        /// </summary>
        private void summarizeConsumerProductExposuresByRoute(
            ConsumerProductExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ConsumerProductExposuresByRouteSection() {
                SectionLabel = getSectionLabel(ConsumerProductExposuresSections.ConsumerProductExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route",
                order
            );
            section.SummarizeChronic(
                data.AllConsumerProducts,
                data.ConsumerProductIndividualExposures,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.SelectedExposureRoutes,
                ExposureType.Chronic,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeConsumerProducts(
            ConsumerProductExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        )  {
            var section = new ConsumerProductExposuresTotalDistributionSection() {
                SectionLabel = getSectionLabel(ConsumerProductExposuresSections.ConsumerProductsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
            section,
                "Exposures per consumer product",
                order
            );
            section.Summarize(
                data.AllConsumerProducts,
                data.ConsumerProductIndividualExposures,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                data.ActiveSubstances,
                _configuration.SelectedExposureRoutes,
                ExposureType.Chronic,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
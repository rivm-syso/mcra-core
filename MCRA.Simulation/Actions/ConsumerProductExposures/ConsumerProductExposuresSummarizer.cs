using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing.Charts;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductExposures {
    public enum ConsumerProductExposuresSections {
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

            // Exposures by exposure route contributions, summary and boxplot
            if (outputSettings.ShouldSummarize(ConsumerProductExposuresSections.ConsumerProductExposuresByRouteSection)) {
                summarizeConsumerProductExposuresByRoute(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }
            // Exposures by consumer product: contributions, summary and boxplot
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
                "Exposures by route and substance",
                order
            );
            section.Summarize(
                data.ActiveSubstances,
                result.ConsumerProductIndividualIntakes,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                result.ConsumerProductExposureUnit,
                [ExposureRoute.Dermal]
            );
            subHeader.SaveSummarySection(section);
        }


        private void summarizeConsumerProducts(
            ConsumerProductExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        )  {
            var section = new TotalConsumerProductsSection() {
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
                [ExposureRoute.Dermal],
                ExposureType.Chronic,
                25,
                75, 5, 95, false
            //   double lowerPercentage,
            //   double upperPercentage,
            //   double uncertaintyLowerBound,
            //   double uncertaintyUpperBound,
            //   bool isPerPerson
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
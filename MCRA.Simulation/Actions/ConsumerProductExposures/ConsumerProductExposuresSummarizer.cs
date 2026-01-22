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
            section.Summarize(actionResult.ConsumerProductIndividualExposures, data.ActiveSubstances);
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
            if (data.ActiveSubstances.Count > 0
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
            if (actionResult.ConsumerProductIndividualExposures != null) {
                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSection;
                        section.SummarizeByRouteUncertainty(
                            actionResult.ConsumerProductIndividualExposures,
                            _configuration.SelectedExposureRoutes,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.ConsumerProductExposureUnit
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSubstanceSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSubstanceSection;
                        section.SummarizeByRouteSubstanceUncertainty(
                            actionResult.ConsumerProductIndividualExposures,
                            _configuration.SelectedExposureRoutes,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.ConsumerProductExposureUnit
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }
            }
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
                result.ConsumerProductIndividualExposures,
                _configuration.SelectedExposureRoutes,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.SelectedPercentiles,
                _configuration.IsPerPerson,
                data.ConsumerProductExposureUnit
            );
            subHeader.SaveSummarySection(section);

            summarizeConsumerProductExposurePercentileByRouteSubstance(
                result,
                data,
                subHeader,
                order
            );
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
            section.Summarize(
                result.ConsumerProductIndividualExposures,
                _configuration.SelectedExposureRoutes,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.SelectedPercentiles,
                _configuration.IsPerPerson,
                data.ConsumerProductExposureUnit);
            subHeader.SaveSummarySection(section);

            summarizeConsumerProductExposurePercentileByRoute(
                result,
                data,
                subHeader,
                order
            );
        }

        private void summarizeConsumerProducts(
            ConsumerProductExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
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
                actionResult.ConsumerProductIndividualExposures,
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

        private void summarizeConsumerProductExposurePercentileByRoute(
            ConsumerProductExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", 0);
            section.SummarizeByRoute(
                result.ConsumerProductIndividualExposures,
                _configuration.SelectedExposureRoutes,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.SelectedPercentiles,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson,
                data.DustExposureUnit
            );
            subHeader?.SaveSummarySection(section);
        }

        private void summarizeConsumerProductExposurePercentileByRouteSubstance(
            ConsumerProductExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
            ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSubstanceSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", order);
            section.SummarizeByRouteSubstance(
                result.ConsumerProductIndividualExposures,
                _configuration.SelectedExposureRoutes,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.SelectedPercentiles,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson,
                data.DustExposureUnit
            );
            subHeader?.SaveSummarySection(section);
        }
    }
}
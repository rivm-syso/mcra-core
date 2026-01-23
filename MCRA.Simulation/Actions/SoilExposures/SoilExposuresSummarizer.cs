using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SoilExposures {
    public enum SoilExposuresSections {
        SoilExposuresByRouteSubstanceSection,
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
            section.Summarize(data.IndividualSoilExposures, data.ActiveSubstances);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by route and substance: contributions, summary and boxplot
            if (outputSettings.ShouldSummarize(SoilExposuresSections.SoilExposuresByRouteSubstanceSection)) {
                summarizeSoilExposuresByRouteSubstance(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }

            // Exposures by route: contributions, summary and boxplot
            if (data.ActiveSubstances.Count > 0
                && outputSettings.ShouldSummarize(SoilExposuresSections.SoilExposuresByRouteSection)
            ) {
                summarizeSoilExposuresByRoute(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }
        }

        public void SummarizeUncertain(
            SoilExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            if (data.IndividualSoilExposures != null) {
                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSection;
                        section.SummarizeByRouteUncertainty(
                            actionResult.IndividualSoilExposures,
                            new List<ExposureRoute>([ExposureRoute.Oral]),
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.SoilExposureUnit
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSubstanceSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSubstanceSection;
                        section.SummarizeByRouteSubstanceUncertainty(
                            actionResult.IndividualSoilExposures,
                            new List<ExposureRoute>([ExposureRoute.Oral]),
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.SoilExposureUnit
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
                new("ExposureUnit", data.SoilExposureUnit.GetShortDisplayName()),
            };
            return result;
        }

        /// <summary>
        /// Soil exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeSoilExposuresByRouteSubstance(
            SoilExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new SoilExposuresByRouteSection() {
                SectionLabel = getSectionLabel(SoilExposuresSections.SoilExposuresByRouteSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route and substance",
                order
            );
            section.Summarize(
                actionResult.IndividualSoilExposures,
                new List<ExposureRoute>([ExposureRoute.Oral]),
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.SelectedPercentiles,
                _configuration.IsPerPerson,
                data.SoilExposureUnit
            );
            subHeader.SaveSummarySection(section);

            summarizeSoilExposurePercentileByRouteSubstance(
                actionResult,
                data,
                subHeader,
                order
            );
        }

        /// <summary>
        /// Exposures by route (boxplot and summary table).
        /// </summary>
        private void summarizeSoilExposuresByRoute(
            SoilExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new SoilExposuresByRouteSection() {
                SectionLabel = getSectionLabel(SoilExposuresSections.SoilExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route",
                order
            );
            section.Summarize(
                data.IndividualSoilExposures,
                new List<ExposureRoute>([ExposureRoute.Oral]),
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.SelectedPercentiles,
                _configuration.IsPerPerson,
                data.SoilExposureUnit);
            subHeader.SaveSummarySection(section);

            summarizeSoilExposurePercentileByRoute(
                result,
                data,
                subHeader,
                order
            );
        }
        private void summarizeSoilExposurePercentileByRoute(
            SoilExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", 0);
            section.SummarizeByRoute(
                result.IndividualSoilExposures,
                new List<ExposureRoute>([ExposureRoute.Oral]),
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.SelectedPercentiles,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson,
                data.SoilExposureUnit
            );
            subHeader?.SaveSummarySection(section);
        }

        private void summarizeSoilExposurePercentileByRouteSubstance(
            SoilExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
            ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSubstanceSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", order);
            section.SummarizeByRouteSubstance(
                result.IndividualSoilExposures,
                new List<ExposureRoute>([ExposureRoute.Oral]),
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.SelectedPercentiles,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson,
                data.SoilExposureUnit
            );
            subHeader?.SaveSummarySection(section);
        }
    }
}
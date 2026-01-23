using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.AirExposures {
    public enum AirExposuresSections {
        AirExposuresByRouteSection,
        AirExposuresByRouteSubstanceSection
    }

    public sealed class AirExposuresSummarizer(AirExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<AirExposuresModuleConfig, AirExposuresActionResult>(config) {
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
            section.Summarize(actionResult.IndividualAirExposures, data.ActiveSubstances);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by route and substance: contributions, summary and boxplot
            if (outputSettings.ShouldSummarize(AirExposuresSections.AirExposuresByRouteSubstanceSection)) {
                summarizeAirExposuresByRouteSubstance(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }
            // Exposures by route: contributions, summary and boxplot
            if (data.ActiveSubstances.Count > 0
                && outputSettings.ShouldSummarize(AirExposuresSections.AirExposuresByRouteSection)
            ) {
                summarizeAirExposuresByRoute(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }
        }

        public void SummarizeUncertain(
            AirExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {

            if (data.IndividualAirExposures != null) {
                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSection;
                        section.SummarizeByRouteUncertainty(
                            actionResult.IndividualAirExposures,
                            _configuration.SelectedExposureRoutes,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.AirExposureUnit
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSubstanceSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSubstanceSection;
                        section.SummarizeByRouteSubstanceUncertainty(
                            actionResult.IndividualAirExposures,
                            _configuration.SelectedExposureRoutes,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.AirExposureUnit
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
                new("ExposureUnit", data.AirExposureUnit.GetShortDisplayName()),
            };
            return result;
        }

        /// <summary>
        /// Exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeAirExposuresByRouteSubstance(
            AirExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new AirExposuresByRouteSubstanceSection() {
                SectionLabel = getSectionLabel(AirExposuresSections.AirExposuresByRouteSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route and substance",
                order
            );
            section.Summarize(
                actionResult.IndividualAirExposures,
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
                data.AirExposureUnit
            );
            subHeader.SaveSummarySection(section);

            summarizeAirExposurePercentileByRouteSubstance(
                actionResult,
                data,
                subHeader,
                order
            );
        }

        /// <summary>
        /// Exposures by route (boxplot and summary table).
        /// </summary>
        private void summarizeAirExposuresByRoute(
            AirExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new AirExposuresByRouteSection() {
                SectionLabel = getSectionLabel(AirExposuresSections.AirExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route",
                order
            );
            section.Summarize(
                result.IndividualAirExposures,
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
                data.AirExposureUnit
            );
            subHeader.SaveSummarySection(section);

            summarizeAirExposurePercentileByRoute(
                result,
                data,
                subHeader,
                order
            );
        }
        private void summarizeAirExposurePercentileByRoute(
            AirExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", 0);
            section.SummarizeByRoute(
                result.IndividualAirExposures,
                _configuration.SelectedExposureRoutes,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.SelectedPercentiles,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson,
                data.AirExposureUnit
            );
            subHeader?.SaveSummarySection(section);
        }

        private void summarizeAirExposurePercentileByRouteSubstance(
            AirExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSubstanceSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", order);
            section.SummarizeByRouteSubstance(
                result.IndividualAirExposures,
                _configuration.SelectedExposureRoutes,
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _configuration.SelectedPercentiles,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                _configuration.IsPerPerson,
                data.AirExposureUnit
            );
            subHeader?.SaveSummarySection(section);
        }
    }
}
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DustExposures {
    public enum DustExposuresSections {
        DustExposuresByRouteSubstanceSection,
        DustExposuresByRouteSection
    }
    public sealed class DustExposuresSummarizer(DustExposuresModuleConfig config)
        : ActionModuleResultsSummarizer<DustExposuresModuleConfig, DustExposuresActionResult>(config) {
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
            section.Summarize(data.IndividualDustExposures, data.ActiveSubstances);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            // Exposures by route and substance: contributions, summary and boxplot
            if (outputSettings.ShouldSummarize(DustExposuresSections.DustExposuresByRouteSubstanceSection)) {
                summarizeDustExposuresByRouteSubstance(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }

            // Exposures by route: contributions, summary and boxplot
            if (data.ActiveSubstances.Count > 0
                && outputSettings.ShouldSummarize(DustExposuresSections.DustExposuresByRouteSection)
            ) {
                summarizeDustExposuresByRoute(
                    actionResult,
                    data,
                    subHeader,
                    order++
                );
            }
        }

        public void SummarizeUncertain(
            DustExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header
        ) {
            if (data.IndividualDustExposures != null) {
                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSection;
                        section.SummarizeByRouteUncertainty(
                            actionResult.IndividualDustExposures,
                            _configuration.SelectedExposureRoutes,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.DustExposureUnit
                        );
                        subHeader.SaveSummarySection(section);
                    }
                }

                {
                    var subHeader = header.GetSubSectionHeader<ExposurePercentilesByRouteSubstanceSection>();
                    if (subHeader != null) {
                        var section = subHeader.GetSummarySection() as ExposurePercentilesByRouteSubstanceSection;
                        section.SummarizeByRouteSubstanceUncertainty(
                            actionResult.IndividualDustExposures,
                            _configuration.SelectedExposureRoutes,
                            data.ActiveSubstances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            _configuration.SelectedPercentiles,
                            _configuration.IsPerPerson,
                            data.DustExposureUnit
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
                new("ExposureUnit", data.DustExposureUnit.GetShortDisplayName()),
            };
            return result;
        }

        /// <summary>
        /// Dust exposures by route and substance (boxplot and summary table).
        /// </summary>
        private void summarizeDustExposuresByRouteSubstance(
            DustExposuresActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustExposuresByRouteSubstanceSection() {
                SectionLabel = getSectionLabel(DustExposuresSections.DustExposuresByRouteSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route and substance",
                order
            );
            section.Summarize(
                actionResult.IndividualDustExposures,
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
                data.DustExposureUnit
            );
            subHeader.SaveSummarySection(section);

            summarizeDustExposurePercentileByRouteSubstance(
                actionResult,
                data,
                subHeader,
                order
            );
        }

        /// <summary>
        /// Dust exposures by route (boxplot and summary table).
        /// </summary>
        private void summarizeDustExposuresByRoute(
            DustExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustExposuresByRouteSection() {
                SectionLabel = getSectionLabel(DustExposuresSections.DustExposuresByRouteSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposures by route",
                order
            );
            section.Summarize(
                data.IndividualDustExposures,
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
                data.DustExposureUnit);
            subHeader.SaveSummarySection(section);

            summarizeDustExposurePercentileByRoute(
                result,
                data,
                subHeader,
                order
            );
        }

        private void summarizeDustExposurePercentileByRoute(
            DustExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", 0);
            section.SummarizeByRoute(
                result.IndividualDustExposures,
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

        private void summarizeDustExposurePercentileByRouteSubstance(
            DustExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
            ) {
            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSubstanceSection();
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", order);
            section.SummarizeByRouteSubstance(
                result.IndividualDustExposures,
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
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.Actions.ExposureMixtures {
    public enum ExposureMixturesSections {
        SubstanceContributionsToComponentSection,
        ExposureBySubstancesInPopulationAndSubGroups,
        ComponentExposuresInPopulationAndSubgroups,
        NetworkAnalysisSection
    }

    public sealed class ExposureMixturesSummarizer : ActionModuleResultsSummarizer<ExposureMixturesModuleConfig, ExposureMixturesActionResult> {

        public ExposureMixturesSummarizer(ExposureMixturesModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, ExposureMixturesActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ExposureMixturesSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput() || result == null) {
                return;
            }

            var outputSummary = new ExposureMixturesSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            var subOrder = 0;

            if (outputSettings.ShouldSummarize(ExposureMixturesSections.SubstanceContributionsToComponentSection)) {
                summarizeSNMUSelectionSection(
                    result.RMSE,
                    result.ComponentRecords,
                    result.UMatrix,
                    result.Substances,
                    result.NumberOfDays,
                    result.NumberOfSelectedDays,
                    result.SubstanceSamplingMethods,
                    result.TotalExposureCutOffPercentile,
                    _configuration.ExposureApproachType,
                    _configuration.MixtureSelectionIterations,
                    _configuration.MixtureSelectionSparsenessConstraint,
                    _configuration.McrCalculationRatioCutOff,
                    _configuration.McrCalculationTotalExposureCutOff,
                    _configuration.ExposureCalculationMethod,
                    _configuration.ExposureType,
                    true,
                    subHeader,
                    subOrder++
                );
            }

            if (_configuration.ExposureType == ExposureType.Chronic) {
                if (_configuration.ClusterMethodType != ClusterMethodType.NoClustering
                    && outputSettings.ShouldSummarize(ExposureMixturesSections.ComponentExposuresInPopulationAndSubgroups)
                ) {
                    summarizeIndividualsExposureSection(
                        result.UMatrix,
                        result.IndividualComponentMatrix,
                        _configuration.ClusterMethodType,
                        true,
                        _configuration.AutomaticallyDeterminationOfClusters,
                        subHeader,
                        subOrder++
                    );
                }

                if (outputSettings.ShouldSummarize(ExposureMixturesSections.ExposureBySubstancesInPopulationAndSubGroups)) {
                    summarizeClusterExposureSection(
                        result.ExposureMatrix,
                        result.IndividualComponentMatrix,
                        result.UMatrix,
                        _configuration.ClusterMethodType,
                        subHeader,
                        subOrder++
                    );
                }
            }

            if (_configuration.NetworkAnalysisType == NetworkAnalysisType.NetworkAnalysis
                && outputSettings.ShouldSummarize(ExposureMixturesSections.NetworkAnalysisSection)) {
                summarizeNetworkAnalysisSection(
                    result.GlassoSelect,
                    result.Substances,
                    subHeader,
                    subOrder++
                );
            }
        }

        /// <summary>
        /// Overview and summary of components
        /// </summary>
        private void summarizeSNMUSelectionSection(
            List<double> rmse,
            List<ComponentRecord> componentRecords,
            GeneralMatrix uMatrix,
            List<Compound> substances,
            int numberOfDays,
            int numberOfSelectedDays,
            IDictionary<Compound, string> substanceSamplingMethods,
            double totalExposureCutOffPercentile,
            ExposureApproachType exposureApproachType,
            int numberOfIterations,
            double sparseness,
            double ratioCutoff,
            double totalExposureCutoff,
            ExposureCalculationMethod exposureCalculationMethod,
            ExposureType exposureType,
            bool removeZeros,
            SectionHeader header,
            int order
        ) {
            var section = new ComponentSelectionOverviewSection() {
                SectionLabel = getSectionLabel(ExposureMixturesSections.SubstanceContributionsToComponentSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Substance contributions to components", order++);
            section.Summarize(
                substances,
                componentRecords,
                rmse,
                uMatrix,
                substanceSamplingMethods,
                exposureApproachType,
                exposureCalculationMethod,
                exposureType,
                totalExposureCutOffPercentile,
                sparseness,
                ratioCutoff,
                totalExposureCutoff,
                numberOfDays,
                numberOfSelectedDays,
                numberOfIterations,
                removeZeros,
                subHeader
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summary of V matrix of SNMU
        /// </summary>
        /// <param name="uMatrix"></param>
        /// <param name="individualMatrix"></param>
        /// <param name="removeZeros"></param>
        /// <param name="clusterMethodType"></param>
        /// <param name="automaticallyDetermineNumberOfClusters"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeIndividualsExposureSection(
            GeneralMatrix uMatrix,
            IndividualMatrix individualMatrix,
            ClusterMethodType clusterMethodType,
            bool removeZeros,
            bool automaticallyDetermineNumberOfClusters,
            SectionHeader header,
            int order
        ) {
            var section = new IndividualsExposureOverviewSection() {
                SectionLabel = getSectionLabel(ExposureMixturesSections.ComponentExposuresInPopulationAndSubgroups)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Component exposures in population and subgroups", order++);
            section.Summarize(
                subHeader,
                uMatrix,
                individualMatrix,
                clusterMethodType,
                automaticallyDetermineNumberOfClusters,
                removeZeros
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summary of cluster analysis
        /// </summary>
        private void summarizeClusterExposureSection(
            ExposureMatrix exposureMatrix,
            IndividualMatrix individualMatrix,
            GeneralMatrix uMatrix,
            ClusterMethodType clusterMethod,
            SectionHeader header,
            int order
        ) {
            var section = new ClusterExposureOverviewSection() {
                SectionLabel = getSectionLabel(ExposureMixturesSections.ExposureBySubstancesInPopulationAndSubGroups)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Exposure by substances in (sub)population", order++);
            section.Summarize(
                exposureMatrix,
                individualMatrix,
                uMatrix,
                clusterMethod,
                subHeader
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Summary of network analysis.
        /// </summary>
        private void summarizeNetworkAnalysisSection(
            double[,] glassoSelect,
            List<Compound> substances,
            SectionHeader header,
            int order
        ) {
            var section = new NetworkAnalysisSection() {
                SectionLabel = getSectionLabel(ExposureMixturesSections.NetworkAnalysisSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Network analysis", order++);
            if (glassoSelect.Max2D() > 0d) {
                section.Summarize(
                    glassoSelect,
                    substances
                );
            }
            subHeader.SaveSummarySection(section);
        }
    }
}

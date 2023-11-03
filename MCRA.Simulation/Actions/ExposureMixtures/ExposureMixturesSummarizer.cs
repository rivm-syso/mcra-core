using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ExposureMixtures {
    public enum ExposureMixturesSections {
        SubstanceContributionsToComponentSection,
        ExposureBySubstancesInPopulationAndSubGroups,
        ComponentExposuresInPopulationAndSubgroups,
        NetworkAnalysisSection
    }

    public sealed class ExposureMixturesSummarizer : ActionResultsSummarizerBase<ExposureMixturesActionResult> {

        public override ActionType ActionType => ActionType.ExposureMixtures;

        private CompositeProgressState _progressState;
        public ExposureMixturesSummarizer(CompositeProgressState progressState = null) {
            _progressState = progressState;
        }

        public override void Summarize(ProjectDto project, ExposureMixturesActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ExposureMixturesSections>(project, ActionType);
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
                    project.MixtureSelectionSettings.ExposureApproachType,
                    project.MixtureSelectionSettings.NumberOfIterations,
                    project.MixtureSelectionSettings.SW,
                    project.MixtureSelectionSettings.RatioCutOff,
                    project.MixtureSelectionSettings.TotalExposureCutOff,
                    project.AssessmentSettings.InternalConcentrationType,
                    project.AssessmentSettings.ExposureType,
                    true,
                    subHeader,
                    subOrder++
                );
            }
            // TODO, remove FirstOrDefault
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                if (project.MixtureSelectionSettings.ClusterMethodType != ClusterMethodType.NoClustering
                    && outputSettings.ShouldSummarize(ExposureMixturesSections.ComponentExposuresInPopulationAndSubgroups)) {
                    summarizeIndividualsExposureSection(
                        result.UMatrix,
                        result.IndividualComponentMatrix,
                        project.MixtureSelectionSettings.ClusterMethodType,
                        true,
                        project.MixtureSelectionSettings.AutomaticallyDeterminationOfClusters,
                        subHeader,
                        subOrder++
                    );
                }

                if (outputSettings.ShouldSummarize(ExposureMixturesSections.ExposureBySubstancesInPopulationAndSubGroups)) {
                    summarizeClusterExposureSection(
                        result.ExposureMatrix,
                        result.IndividualComponentMatrix,
                        result.UMatrix,
                        project.MixtureSelectionSettings.ClusterMethodType,
                        subHeader,
                        subOrder++
                    );
                }
                if (project.MixtureSelectionSettings.NetworkAnalysisType == NetworkAnalysisType.NetworkAnalysis
                    && outputSettings.ShouldSummarize(ExposureMixturesSections.NetworkAnalysisSection)) {
                    summarizeNetworkAnalysisSection(
                        result.GlassoSelect,
                        result.Substances,
                        subHeader,
                        subOrder++
                    );
                }
            }
        }

        /// <summary>
        /// Overview and summary of components
        /// </summary>
        /// <param name="rmse"></param>
        /// <param name="componentRecords"></param>
        /// <param name="uMatrix"></param>
        /// <param name="substances"></param>
        /// <param name="numberOfDays"></param>
        /// <param name="numberOfSelectedDays"></param>
        /// <param name="totalExposureCutOffPercentile"></param>
        /// <param name="exposureApproachType"></param>
        /// <param name="numberOfIterations"></param>
        /// <param name="sparseness"></param>
        /// <param name="ratioCutoff"></param>
        /// <param name="totalExposureCutoff"></param>
        /// <param name="internalConcentrationType"></param>
        /// <param name="exposureType"></param>
        /// <param name="removeZeros"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        /// 
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
            InternalConcentrationType internalConcentrationType,
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
                internalConcentrationType,
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
        /// <param name="exposureMatrix"></param>
        /// <param name="individualMatrix"></param>
        /// <param name="uMatrix"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
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
        /// Summary of network analysis
        /// </summary>
        /// <param name="glassoSelect"></param>
        /// <param name="substances"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
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
            section.Summarize(
                glassoSelect,
                substances
            );
            subHeader.SaveSummarySection(section);
        }

    }
}


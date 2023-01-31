using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.HClustCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.KMeansCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.ExposureMixtures {

    [ActionType(ActionType.ExposureMixtures)]
    public sealed class ExposureMixturesActionCalculator : ActionCalculatorBase<ExposureMixturesActionResult> {

        public ExposureMixturesActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isTargetLevelInternal = _project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal;
            var isMonitoringConcentrations = _project.AssessmentSettings.InternalConcentrationType == InternalConcentrationType.MonitoringConcentration;
            if (!isTargetLevelInternal) {
                _project.AssessmentSettings.InternalConcentrationType = InternalConcentrationType.ModelledConcentration;
                isMonitoringConcentrations = false;
            }
            var requireRpfs = _project.MixtureSelectionSettings.ExposureApproachType == ExposureApproachType.RiskBased;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = requireRpfs;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = requireRpfs;
            _actionInputRequirements[ActionType.Effects].IsVisible = requireRpfs;
            _actionInputRequirements[ActionType.Effects].IsRequired = requireRpfs;
            _actionInputRequirements[ActionType.TargetExposures].IsVisible = isTargetLevelInternal && !isMonitoringConcentrations;
            _actionInputRequirements[ActionType.TargetExposures].IsRequired = isTargetLevelInternal && !isMonitoringConcentrations;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = !isTargetLevelInternal;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = !isTargetLevelInternal;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsRequired = isMonitoringConcentrations && isTargetLevelInternal;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsVisible = isMonitoringConcentrations && isTargetLevelInternal;
            if (_project.ActionType == ActionType.ExposureMixtures) {
                _project.OutputDetailSettings.MaximumCumulativeRatioCutOff = _project.MixtureSelectionSettings.RatioCutOff;
            }
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new ExposureMixturesSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ExposureMixturesSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override ExposureMixturesActionResult run(ActionData data, CompositeProgressState progressReport) {
            var settings = new ExposureMixturesModuleSettings(_project);
            var localProgress = progressReport.NewProgressState(100);

            var exposureMatrixBuilder = new ExposureMatrixBuilder(
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                _project.AssessmentSettings.ExposureType,
                _project.SubsetSettings.IsPerPerson,
                settings.ExposureApproachType,
                settings.TotalExposureCutOff,
                settings.RatioCutOff
            );

            if (settings.RatioCutOff >= data.ActiveSubstances.Count) {
                throw new Exception($"The specified ratio cutoff for MCR is: {settings.RatioCutOff} where the maximum MCR is: {data.ActiveSubstances.Count}.");
            }

            ExposureMatrix exposureMatrix;
            if (_project.EffectSettings.TargetDoseLevelType == TargetLevelType.External) {
                exposureMatrix = exposureMatrixBuilder
                    .Compute(data.DietaryIndividualDayIntakes);
            } else {
                if (settings.InternalConcentrationType == InternalConcentrationType.ModelledConcentration) {
                    exposureMatrix = exposureMatrixBuilder
                        .Compute(
                            data.AggregateIndividualDayExposures,
                            data.AggregateIndividualExposures
                        );
                } else {
                    exposureMatrix = exposureMatrixBuilder
                       .Compute(
                           data.HbmIndividualDayConcentrations,
                           data.HbmIndividualConcentrations
                       );
                }
            }
            if (exposureMatrix == null) {
                return null;
            }

            var (nmfExposureMatrix, totalExposureCutOffPercentile) = exposureMatrixBuilder.Compute(exposureMatrix);
            // NMF random generator
            var nmfRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.MIX_NmfInitialisation));

            localProgress.Update("Non negative matrix factorization", 20);
            var nmfCalculator = new NmfCalculator(settings);

            var (componentRecords, UMatrix, VMatrix, rmse) = nmfCalculator
                .Compute(nmfExposureMatrix.Exposures, nmfRandomGenerator, new ProgressState());

            var individualComponentMatrix = new IndividualMatrix();
            if (settings.ClusterMethodType != ClusterMethodType.NoClustering) {
                individualComponentMatrix = new IndividualMatrix() {
                    VMatrix = VMatrix.Transpose(),
                    Individuals = nmfExposureMatrix.Individuals
                };
                if (settings.ClusterMethodType == ClusterMethodType.Kmeans) {
                    var kmeansCalculator = new KMeansCalculator(settings.NumberOfClusters);
                    individualComponentMatrix.ClusterResult = kmeansCalculator.Compute(individualComponentMatrix, UMatrix);
                } else {
                    var hclustCalculator = new HClustCalculator(settings.NumberOfClusters, settings.AutomaticallyDeterminationOfClusters);
                    individualComponentMatrix.ClusterResult = hclustCalculator.Compute(individualComponentMatrix, UMatrix);
                }
            }
            var glassoResult = new double[UMatrix.RowDimension, UMatrix.ColumnDimension];
            if (settings.NetworkAnalysisType == NetworkAnalysisType.NetworkAnalysis) {
                var networkAnalysisCalculator = new NetworkAnalysisCalculator();
                glassoResult = networkAnalysisCalculator.Compute(nmfExposureMatrix.Exposures);
            }

            var result = new ExposureMixturesActionResult() {
                ComponentRecords = componentRecords,
                UMatrix = UMatrix,
                IndividualComponentMatrix = individualComponentMatrix,
                Substances = nmfExposureMatrix.Substances,
                NumberOfDays = exposureMatrix.Exposures.ColumnDimension,
                NumberOfSelectedDays = nmfExposureMatrix.Individuals.Count,
                TotalExposureCutOffPercentile = totalExposureCutOffPercentile,
                ExposureMatrix = nmfExposureMatrix,
                RMSE = rmse,
                GlassoSelect = glassoResult,
            };

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(ExposureMixturesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update($"Summarizing {ActionType.GetDisplayName(true)}", 0);
            var summarizer = new ExposureMixturesSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}

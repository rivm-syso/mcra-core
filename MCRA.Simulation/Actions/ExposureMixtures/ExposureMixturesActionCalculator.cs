using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
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

            if (settings.RatioCutOff >= data.ActiveSubstances.Count) {
                // RatioCutOff should be lower than the number of (active) substances
                throw new Exception($"The specified ratio cutoff for MCR is {settings.RatioCutOff} where the maximum MCR is {data.ActiveSubstances.Count}.");
            }

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

            ExposureMatrix exposureMatrix;
            Dictionary<Compound, string> samplingMethods = null;
            if (_project.EffectSettings.TargetDoseLevelType == TargetLevelType.External) {
                // Mixtures analysis from external (dietary) concentrations
                exposureMatrix = exposureMatrixBuilder
                    .Compute(data.DietaryIndividualDayIntakes);
            } else {
                // Mixtures analysis from internal concentrations
                if (settings.InternalConcentrationType == InternalConcentrationType.ModelledConcentration) {
                    // Mixtures analysis from modelled internal concentrations
                    exposureMatrix = exposureMatrixBuilder
                        .Compute(
                            data.AggregateIndividualDayExposures,
                            data.AggregateIndividualExposures
                        );
                } else {
                    // Mixtures analysis from internal concentrations obtained from human biomonitoring
                    exposureMatrix = exposureMatrixBuilder
                       .Compute(
                           data.HbmIndividualDayConcentrations,
                           data.HbmIndividualConcentrations
                       );

                    // Retrieve the source sampling method for each substance
                    // TODO: in the future this dictionary should be based on the target (matrix)
                    // per substance instead of the source sampling method.
                    var hbmIndividualDayConcentrationBySubstanceRecords = data.HbmIndividualDayConcentrations
                        .Select(c => c.ConcentrationsBySubstance)
                        .ToList();
                    samplingMethods = new Dictionary<Compound, string>();
                    foreach (var item in exposureMatrix.Substances) {
                        foreach (var dict in hbmIndividualDayConcentrationBySubstanceRecords) {
                            if (dict.TryGetValue(item, out var record) && !samplingMethods.TryGetValue(item, out var value)) {
                                samplingMethods.Add(item, string.Join(" ,", record.SourceSamplingMethods.Select(c => c.Name).ToList()));
                            }
                        }
                    }
                    samplingMethods = samplingMethods.Select(c => c.Value).Distinct().Count() == 1 ? null : samplingMethods;
                }
            }

            // Create NMF exposure matrix with only the exposures above the cutoff percentile
            // (including the exposure associated with the cutoff percentile.
            var (nmfExposureMatrix, totalExposureCutOffPercentile) = exposureMatrixBuilder.Compute(exposureMatrix);

            // NMF random generator
            var nmfRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.MIX_NmfInitialisation));

            // NNMF calculation
            localProgress.Update("Non negative matrix factorization", 20);
            var nmfCalculator = new NmfCalculator(settings);
            var (componentRecords, UMatrix, VMatrix, rmse) = nmfCalculator
                .Compute(nmfExposureMatrix.Exposures, nmfRandomGenerator, new ProgressState());

            var individualComponentMatrix = new IndividualMatrix() {
                VMatrix = VMatrix.Transpose(),
                Individuals = nmfExposureMatrix.Individuals
            };

            if (settings.ClusterMethodType == ClusterMethodType.Kmeans) {
                // KMeans clustering
                var kmeansCalculator = new KMeansCalculator(settings.NumberOfClusters);
                individualComponentMatrix.ClusterResult = kmeansCalculator.Compute(individualComponentMatrix, UMatrix);
            } else if (settings.ClusterMethodType == ClusterMethodType.Hierarchical) {
                // Hierarchical clustering
                var hclustCalculator = new HClustCalculator(settings.NumberOfClusters, settings.AutomaticallyDeterminationOfClusters);
                individualComponentMatrix.ClusterResult = hclustCalculator.Compute(individualComponentMatrix, UMatrix);
            }

            var glassoResult = new double[UMatrix.RowDimension, UMatrix.ColumnDimension];
            if (settings.NetworkAnalysisType == NetworkAnalysisType.NetworkAnalysis) {
                // Network analysis
                var networkAnalysisCalculator = new NetworkAnalysisCalculator(settings.IsLogTransform);
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
                SubstanceSamplingMethods = samplingMethods
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

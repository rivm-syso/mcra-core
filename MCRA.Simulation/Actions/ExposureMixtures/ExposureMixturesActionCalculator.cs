﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
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
        private ExposureMixturesModuleConfig ModuleConfig => (ExposureMixturesModuleConfig)_moduleSettings;

        public ExposureMixturesActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isTargetLevelInternal = ModuleConfig.TargetDoseLevelType == TargetLevelType.Internal;
            var isMonitoringConcentrations = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration;
            if (!isTargetLevelInternal) {
                ModuleConfig.ExposureCalculationMethod = ExposureCalculationMethod.ModelledConcentration;
                isMonitoringConcentrations = false;
            }
            var requireRpfs = ModuleConfig.ExposureApproachType == ExposureApproachType.RiskBased;
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
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new ExposureMixturesSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ExposureMixturesSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override ExposureMixturesActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            if (ModuleConfig.McrCalculationRatioCutOff >= data.ActiveSubstances.Count) {
                // RatioCutOff should be lower than the number of (active) substances
                throw new Exception($"The specified ratio cutoff for MCR is {ModuleConfig.McrCalculationRatioCutOff} where the maximum MCR is {data.ActiveSubstances.Count}.");
            }

            var exposureMatrixBuilder = new ExposureMatrixBuilder(
                data.ActiveSubstances,
                data.CorrectedRelativePotencyFactors,
                data.MembershipProbabilities,
                ModuleConfig.ExposureType,
                ModuleConfig.IsPerPerson,
                ModuleConfig.ExposureApproachType,
                ModuleConfig.McrCalculationTotalExposureCutOff,
                ModuleConfig.McrCalculationRatioCutOff
            );

            ExposureMatrix exposureMatrix;
            Dictionary<Compound, string> samplingMethods = null;
            if (ModuleConfig.TargetDoseLevelType == TargetLevelType.External) {
                // Mixtures analysis from external (dietary) concentrations
                exposureMatrix = exposureMatrixBuilder
                    .Compute(
                        data.DietaryIndividualDayIntakes,
                        data.DietaryExposureUnit
                    );
            } else {
                // Mixtures analysis from internal concentrations
                if (ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration) {
                    // Mixtures analysis from modelled internal concentrations
                    exposureMatrix = exposureMatrixBuilder
                        .Compute(
                            data.AggregateIndividualDayExposures,
                            data.AggregateIndividualExposures,
                            data.TargetExposureUnit
                        );
                } else {
                    // Mixtures analysis from internal concentrations obtained from human biomonitoring
                    exposureMatrix = exposureMatrixBuilder
                       .Compute(
                           data.HbmIndividualDayCollections,
                           data.HbmIndividualCollections
                       );

                    // Retrieve the source sampling method for each substance
                    // TODO: in the future this dictionary should be based on the target (matrix)
                    // per substance instead of the source sampling method.
                    samplingMethods = [];
                    var hbmIndividualDayConcentrationBySubstanceRecords = data.HbmIndividualDayCollections
                        .SelectMany(c => c.HbmIndividualDayConcentrations)
                        .Select(c => c.ConcentrationsBySubstance)
                        .ToList();
                    foreach (var item in exposureMatrix.RowRecords.Values) {
                        foreach (var dict in hbmIndividualDayConcentrationBySubstanceRecords) {
                            if (dict.TryGetValue(item.Substance, out var record) && !samplingMethods.TryGetValue(item.Substance, out var value)) {
                                samplingMethods.Add(item.Substance, string.Join(" ,", record.SourceSamplingMethods.Select(c => c.Name).ToList()));
                            }
                        }
                    }
                    samplingMethods = samplingMethods.Select(c => c.Value).Distinct().Count() == 1
                        ? null : samplingMethods;
                }
            }

            // Create NMF exposure matrix with only the exposures above the cutoff percentile
            // (including the exposure associated with the cutoff percentile.
            var (nmfExposureMatrix, totalExposureCutOffPercentile) = exposureMatrixBuilder
                .Compute(exposureMatrix);

            // NMF random generator
            var nmfRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(ModuleConfig.MixtureSelectionRandomSeed, (int)RandomSource.MIX_NmfInitialisation)
            );

            // NNMF calculation
            localProgress.Update("Non negative matrix factorization", 20);
            var nmfCalculator = new NmfCalculator(
                ModuleConfig.MixtureSelectionIterations,
                ModuleConfig.NumberOfMixtures,
                ModuleConfig.MixtureSelectionSparsenessConstraint,
                ModuleConfig.MixtureSelectionConvergenceCriterium
            );

            var (componentRecords, uMatrix, vMatrix, rmse) = nmfCalculator
                .Compute(nmfExposureMatrix.Exposures, nmfRandomGenerator, new ProgressState());

            var individualComponentMatrix = new IndividualMatrix() {
                VMatrix = vMatrix.Transpose(),
                SimulatedIndividuals = nmfExposureMatrix.SimulatedIndividuals
            };

            if (ModuleConfig.ClusterMethodType == ClusterMethodType.Kmeans) {
                // KMeans clustering
                var kmeansCalculator = new KMeansCalculator(ModuleConfig.NumberOfClusters);
                individualComponentMatrix.ClusterResult = kmeansCalculator
                    .Compute(individualComponentMatrix, uMatrix);
            } else if (ModuleConfig.ClusterMethodType == ClusterMethodType.Hierarchical) {
                // Hierarchical clustering
                var hclustCalculator = new HClustCalculator(
                    ModuleConfig.NumberOfClusters,
                    ModuleConfig.AutomaticallyDeterminationOfClusters
                );
                individualComponentMatrix.ClusterResult = hclustCalculator
                    .Compute(individualComponentMatrix, uMatrix);
            }

            var glassoResult = new double[uMatrix.RowDimension, uMatrix.ColumnDimension];
            if (ModuleConfig.NetworkAnalysisType == NetworkAnalysisType.NetworkAnalysis && ModuleConfig.ExposureType == ExposureType.Chronic) {
                // Network analysis
                var networkAnalysisCalculator = new NetworkAnalysisCalculator(ModuleConfig.IsLogTransform);
                glassoResult = networkAnalysisCalculator.Compute(nmfExposureMatrix.Exposures);
            }

            var result = new ExposureMixturesActionResult() {
                ComponentRecords = componentRecords,
                UMatrix = uMatrix,
                IndividualComponentMatrix = individualComponentMatrix,
                Substances = nmfExposureMatrix.RowRecords.Values.Select(c => c.Substance).ToList(),
                NumberOfDays = exposureMatrix.Exposures.ColumnDimension,
                NumberOfSelectedDays = nmfExposureMatrix.SimulatedIndividuals.Count,
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
            var summarizer = new ExposureMixturesSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}

﻿using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.BloodCorrectionCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmConcentrationModelCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationsPruning;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectsImputationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCombinationCalculation;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    [ActionType(ActionType.HumanMonitoringAnalysis)]
    public class HumanMonitoringAnalysisActionCalculator : ActionCalculatorBase<HumanMonitoringAnalysisActionResult> {
        private HumanMonitoringAnalysisModuleConfig ModuleConfig => (HumanMonitoringAnalysisModuleConfig)_moduleSettings;

        public HumanMonitoringAnalysisActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isMultiple = ModuleConfig.MultipleSubstances;
            var isCumulative = isMultiple && ModuleConfig.Cumulative;
            var isRiskBasedMcr = isMultiple && ModuleConfig.McrAnalysis
                && ModuleConfig.McrExposureApproachType == ExposureApproachType.RiskBased;
            var useKineticModels = ModuleConfig.ApplyKineticConversions;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
            var applyExposureBiomarkerConversions = ModuleConfig.ApplyExposureBiomarkerConversions;
            _actionInputRequirements[ActionType.ExposureBiomarkerConversions].IsRequired = applyExposureBiomarkerConversions;
            _actionInputRequirements[ActionType.ExposureBiomarkerConversions].IsVisible = applyExposureBiomarkerConversions;
            _actionInputRequirements[ActionType.KineticConversionFactors].IsRequired = useKineticModels;
            _actionInputRequirements[ActionType.KineticConversionFactors].IsVisible = useKineticModels;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleHbmIndividuals) {
                result.Add(UncertaintySource.HbmNonDetectImputation);
                result.Add(UncertaintySource.HbmMissingValueImputation);
                result.Add(UncertaintySource.ExposureBiomarkerConversion);
            }
            return result;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new HumanMonitoringAnalysisSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HumanMonitoringAnalysisSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override HumanMonitoringAnalysisActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            return compute(data, localProgress, true);
        }

        protected override HumanMonitoringAnalysisActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            return compute(data, localProgress, false, factorialSet, uncertaintySourceGenerators);
        }

        private HumanMonitoringAnalysisActionResult compute(
            ActionData data,
            ProgressState localProgress,
            bool isMcrAnalyis,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            // Create HBM concentration models
            var concentrationModelsBuilder = new HbmConcentrationModelBuilder();
            var concentrationModels = ModuleConfig.NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit
                ? concentrationModelsBuilder.Create(
                    data.HbmSampleSubstanceCollections,
                    ModuleConfig.HbmNonDetectsHandlingMethod,
                    ModuleConfig.HbmFractionOfLor
                )
                : null;

            // Imputation of censored values
            var nonDetectsImputationCalculator = new HbmNonDetectsImputationCalculator(
                ModuleConfig.NonDetectImputationMethod,
                ModuleConfig.HbmNonDetectsHandlingMethod,
                ModuleConfig.HbmFractionOfLor
            );

            var imputedNonDetectsSubstanceCollection = nonDetectsImputationCalculator
                .ImputeNonDetects(
                    data.HbmSampleSubstanceCollections,
                    ModuleConfig.NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit ? concentrationModels : null,
                    factorialSet?.Contains(UncertaintySource.HbmNonDetectImputation) ?? false
                        ? RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.HbmNonDetectImputation].Seed, (int)RandomSource.HBM_CensoredValueImputation)
                        : RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.HBM_CensoredValueImputation)
            );

            // Impute missing values
            var missingValueImputationCalculator = HbmMissingValueImputationCalculatorFactory
                .Create(ModuleConfig.MissingValueImputationMethod);

            var imputedMissingValuesSubstanceCollection = missingValueImputationCalculator
                .ImputeMissingValues(
                    imputedNonDetectsSubstanceCollection,
                    ModuleConfig.MissingValueCutOff,
                    factorialSet?.Contains(UncertaintySource.HbmMissingValueImputation) ?? false
                        ? RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.HbmMissingValueImputation].Seed, (int)RandomSource.HBM_MissingValueImputation)
                        : RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.HBM_MissingValueImputation)
            );

            // Standardize blood concentrations (express soluble substances per lipid content)
            var standardisedSubstanceCollections = imputedMissingValuesSubstanceCollection;
            if (ModuleConfig.StandardiseBlood) {
                var substancesExcludedFromLipidStandardisation = ModuleConfig.StandardiseBloodExcludeSubstances ? ModuleConfig.StandardiseBloodExcludedSubstancesSubset : [];
                var lipidContentCorrector = BloodCorrectionCalculatorFactory.Create(ModuleConfig.StandardiseBloodMethod, substancesExcludedFromLipidStandardisation);
                standardisedSubstanceCollections = lipidContentCorrector
                    .ComputeResidueCorrection(
                        imputedMissingValuesSubstanceCollection
                    );
            }

            // Normalise by specific gravity or standardise by creatinine concentration
            if (ModuleConfig.StandardiseUrine) {
                var substancesExcludedFromUrineStandardisation = ModuleConfig.StandardiseUrineExcludeSubstances
                    ? ModuleConfig.StandardiseUrineExcludedSubstancesSubset : [];
                var urineCorrectorCalculator = UrineCorrectionCalculatorFactory
                    .Create(
                        ModuleConfig.StandardiseUrineMethod,
                        ModuleConfig.SpecificGravityConversionFactor,
                        substancesExcludedFromUrineStandardisation
                    );
                standardisedSubstanceCollections = urineCorrectorCalculator
                    .ComputeResidueCorrection(
                        standardisedSubstanceCollections
                    );
            }

            // Create individual day collections, imputed for missing body weights
            var simulatedIndividualDays = HbmIndividualDaysGenerator
                .CreateSimulatedIndividualDays(data.HbmSampleSubstanceCollections)
                .ToList();
            IndividualDaysGenerator.ImputeBodyWeight(simulatedIndividualDays.Select(d => d.SimulatedIndividual).Distinct());

            // Compute HBM individual day concentration collections (per combination of matrix and expression type)
            var hbmIndividualDayCollections = new List<HbmIndividualDayCollection>();
            foreach (var standardisedSubstanceCollection in standardisedSubstanceCollections) {
                var hbmIndividualDayConcentrationCalculator = new HbmIndividualDayConcentrationsCalculator();
                var hbmIndividualDayCollection = hbmIndividualDayConcentrationCalculator
                    .Calculate(
                        standardisedSubstanceCollection,
                        simulatedIndividualDays,
                        data.AllCompounds
                    );
                hbmIndividualDayCollections.Add(hbmIndividualDayCollection);
            }

            // Combine urine with different sampling methods
            hbmIndividualDayCollections = UrineCombinationCalculator.Combine(
                hbmIndividualDayCollections,
                simulatedIndividualDays
            );

            var result = new HumanMonitoringAnalysisActionResult();

            if (ModuleConfig.ApplyExposureBiomarkerConversions || ModuleConfig.ApplyKineticConversions) {

                // Before conversion, filter on complete cases
                hbmIndividualDayCollections = GetCompleteCases(
                    hbmIndividualDayCollections,
                    ModuleConfig.StandardiseBloodExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase),
                    ModuleConfig.StandardiseUrineExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase)
                );

                // Store the day concentrations derived for the measured matrices
                result.HbmMeasuredMatrixIndividualDayCollections = hbmIndividualDayCollections;

                // Apply exposure biomarker conversion.
                if (ModuleConfig.ApplyExposureBiomarkerConversions) {
                    var seed = factorialSet?.Contains(UncertaintySource.ExposureBiomarkerConversion) ?? false
                        ? RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.ExposureBiomarkerConversion].Seed, (int)RandomSource.HBM_ExposureBiomarkerConversion)
                        : RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.HBM_ExposureBiomarkerConversion);
                    var conversionCalculator = new ExposureBiomarkerConversionCalculator(data.ExposureBiomarkerConversionModels);
                    hbmIndividualDayCollections = conversionCalculator.Convert(
                        hbmIndividualDayCollections,
                        seed
                    );
                }

                if (ModuleConfig.ApplyKineticConversions) {
                    var substances = data.ActiveSubstances ?? data.AllCompounds;

                    if (ModuleConfig.HbmConvertToSingleTargetMatrix) {
                        // Kinetic conversions to a single target
                        hbmIndividualDayCollections = HbmSingleTargetExtrapolationCalculator
                            .Calculate(
                                hbmIndividualDayCollections,
                                data.KineticConversionFactorModels,
                                simulatedIndividualDays,
                                substances,
                                ModuleConfig.TargetDoseLevelType,
                                ModuleConfig.TargetMatrix
                            );
                    } else {
                        // Kinetic conversions for different from-to matrix combinations
                        hbmIndividualDayCollections = HbmMultipleTargetExtrapolationCalculator
                            .Calculate(
                                hbmIndividualDayCollections,
                                data.KineticConversionFactorModels,
                                simulatedIndividualDays,
                                substances
                            );
                    }
                }
            }

            // Filter on active substances
            var activeSubstances = data.ActiveSubstances;
            hbmIndividualDayCollections = hbmIndividualDayCollections.Select(c => c.Clone()).ToList();
            hbmIndividualDayCollections.ForEach(collection => {
                foreach (var item in collection.HbmIndividualDayConcentrations) {
                    item.ConcentrationsBySubstance = item.ConcentrationsBySubstance.Where(i => activeSubstances.Contains(i.Key)).ToDictionary(d => d.Key, d => d.Value);
                }
            });

            // Remove all individualDays containing missing values.
            var individualDayCollections = GetCompleteCases(
                hbmIndividualDayCollections,
                ModuleConfig.StandardiseBloodExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase),
                ModuleConfig.StandardiseUrineExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase)
            );

            // Throw an exception if we all individual days were removed due to missing substance concentrations
            if (!individualDayCollections.SelectMany(c => c.HbmIndividualDayConcentrations).Any()) {
                throw new Exception("All HBM individual day records were removed for having non-imputed missing substance concentrations.");
            }

            // For chronic assessments, compute average individual concentrations
            List<HbmIndividualCollection> individualCollections = null;
            if (ModuleConfig.ExposureType == ExposureType.Chronic) {
                var individualConcentrationsCalculator = new HbmIndividualConcentrationsCalculator();
                individualCollections = individualConcentrationsCalculator
                    .Calculate(
                        data.ActiveSubstances,
                        individualDayCollections
                    );
                if (result.HbmMeasuredMatrixIndividualDayCollections?.Count > 0) {
                    result.HbmMeasuredMatrixIndividualCollections = individualConcentrationsCalculator
                        .Calculate(
                            data.AllCompounds,
                            result.HbmMeasuredMatrixIndividualDayCollections
                        );
                }
            }

            // Compute cumulative concentrations (only for single target)
            if (individualDayCollections.Count == 1) {
                if (data.CorrectedRelativePotencyFactors != null) {
                    if (ModuleConfig.ExposureType == ExposureType.Chronic) {
                        // For cumulative assessments, compute cumulative individual concentrations
                        var hbmCumulativeIndividualCalculator = new HbmCumulativeIndividualConcentrationCalculator();
                        var cumulativeIndividualCollection = hbmCumulativeIndividualCalculator
                            .Calculate(
                                individualCollections,
                                data.ActiveSubstances,
                                data.CorrectedRelativePotencyFactors
                            );
                        result.HbmCumulativeIndividualCollection = cumulativeIndividualCollection;
                    } else {
                        // For cumulative assessments, compute cumulative individual day concentrations
                        var hbmCumulativeIndividualDayCalculator = new HbmCumulativeIndividualDayConcentrationCalculator();
                        var cumulativeIndividualDayCollection = hbmCumulativeIndividualDayCalculator
                            .Calculate(
                                individualDayCollections,
                                data.ActiveSubstances,
                                data.CorrectedRelativePotencyFactors
                            );
                        result.HbmCumulativeIndividualDayCollection = cumulativeIndividualDayCollection;
                    }
                }
            }

            // MCR analysis
            if (ModuleConfig.McrAnalysis
                && data.ActiveSubstances.Count > 1
                && uncertaintySourceGenerators == null
                && isMcrAnalyis
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    data.ActiveSubstances,
                    data.ReferenceSubstance == null ? data.ActiveSubstances.ToDictionary(r => r, r => 1D) : data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    ModuleConfig.ExposureType,
                    ModuleConfig.IsPerPerson,
                    ModuleConfig.McrExposureApproachType,
                    ModuleConfig.McrCalculationTotalExposureCutOff,
                    ModuleConfig.McrCalculationRatioCutOff
                 );
                result.ExposureMatrix = exposureMatrixBuilder.Compute(
                    individualDayCollections,
                    individualCollections
                );
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.ExposureMatrix);
            }

            localProgress.Update(100);
            result.HbmIndividualDayConcentrations = individualDayCollections;
            result.HbmIndividualConcentrations = individualCollections;
            result.HbmConcentrationModels = concentrationModels;
            return result;
        }

        private static List<HbmIndividualDayCollection> GetCompleteCases(
            ICollection<HbmIndividualDayCollection> individualDayCollections,
            HashSet<string> standardiseBloodExcludedSubstancesSubset,
            HashSet<string> standardiseUrineExcludedSubstancesSubset
        ) {

            // Prune individual day collections so that for each substance
            // is reported for at most one expression type for each matrix.
            // E.g., for substance x, blood concentrations should be expressed
            // either per L blood or per g lipids.
            var hbmIndividualDayConcentrationsCollectionPruner = new HbmIndividualDayConcentrationsCollectionPruner(
                standardiseBloodExcludedSubstancesSubset,
                standardiseUrineExcludedSubstancesSubset
            );
            var prunedIndividualDayCollections = hbmIndividualDayConcentrationsCollectionPruner
                .Prune(individualDayCollections);

            // Remove all individualDays containing missing values.
            var result = new List<HbmIndividualDayCollection>();
            foreach (var collection in prunedIndividualDayCollections) {
                var remainingSubstances = collection.HbmIndividualDayConcentrations
                    .SelectMany(c => c.ConcentrationsBySubstance.Keys)
                    .Distinct()
                    .ToList();
                var records = collection.HbmIndividualDayConcentrations
                    .Select(individualDay => {
                        var individualDaySubstances = individualDay.ConcentrationsBySubstance.Keys.ToList();
                        return !remainingSubstances.Except(individualDaySubstances).Any() ? individualDay : null;
                    })
                    .Where(c => c != null)
                    .ToList();
                result.Add(new HbmIndividualDayCollection() {
                    TargetUnit = collection.TargetUnit,
                    HbmIndividualDayConcentrations = records,
                });
            }

            // Get simulated individual ids with concentrations for all matrices
            var completeCases = result
                .SelectMany(
                    r => r.HbmIndividualDayConcentrations,
                    (r, idi) => (r.Target, idi.SimulatedIndividualDayId)
                )
                .GroupBy(r => r.SimulatedIndividualDayId)
                .Where(r => r.Count() == result.Count)
                .Select(r => r.Key)
                .ToHashSet();

            var completeResults = new List<HbmIndividualDayCollection>();
            foreach (var collection in result) {
                var individualDayConcentrations = collection.HbmIndividualDayConcentrations
                    .Where(r => completeCases.Contains(r.SimulatedIndividualDayId))
                    .ToList();
                completeResults.Add(new HbmIndividualDayCollection() {
                    TargetUnit = collection.TargetUnit,
                    HbmIndividualDayConcentrations = individualDayConcentrations
                });
            }
            return completeResults;
        }

        protected override void updateSimulationData(ActionData data, HumanMonitoringAnalysisActionResult result) {
            data.HbmIndividualDayCollections = result.HbmIndividualDayConcentrations;
            data.HbmIndividualCollections = result.HbmIndividualConcentrations;
            data.HbmCumulativeIndividualCollection = result.HbmCumulativeIndividualCollection;
            data.HbmCumulativeIndividualDayCollection = result.HbmCumulativeIndividualDayCollection;
        }

        protected override void summarizeActionResult(HumanMonitoringAnalysisActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing human monitoring analysis results", 0);
            var summarizer = new HumanMonitoringAnalysisSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, HumanMonitoringAnalysisActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new HumanMonitoringAnalysisSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, HumanMonitoringAnalysisActionResult result) {
            updateSimulationData(data, result);
        }
    }
}

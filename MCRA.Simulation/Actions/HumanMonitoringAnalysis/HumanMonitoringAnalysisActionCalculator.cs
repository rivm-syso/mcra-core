using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
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

        public HumanMonitoringAnalysisActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isMultiple = _project.AssessmentSettings.MultipleSubstances;
            var isCumulative = isMultiple && _project.AssessmentSettings.Cumulative;
            var isRiskBasedMcr = isMultiple && _project.HumanMonitoringSettings.AnalyseMcr
                && _project.HumanMonitoringSettings.ExposureApproachType == ExposureApproachType.RiskBased;
            var useKineticModels = _project.HumanMonitoringSettings.ApplyKineticConversions;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.KineticModels].IsRequired = useKineticModels;
            _actionInputRequirements[ActionType.KineticModels].IsVisible = useKineticModels;
            var applyExposureBiomarkerConversions = _project.HumanMonitoringSettings.ApplyExposureBiomarkerConversions;
            _actionInputRequirements[ActionType.ExposureBiomarkerConversions].IsRequired = applyExposureBiomarkerConversions;
            _actionInputRequirements[ActionType.ExposureBiomarkerConversions].IsVisible = applyExposureBiomarkerConversions;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ResampleHBMIndividuals) {
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
            var summarizer = new HumanMonitoringAnalysisSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override HumanMonitoringAnalysisActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var settings = new HumanMonitoringAnalysisModuleSettings(_project, false);
            return compute(data, localProgress, settings, true);
        }

        protected override HumanMonitoringAnalysisActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var settings = new HumanMonitoringAnalysisModuleSettings(_project, true);
            return compute(data, localProgress, settings, false, factorialSet, uncertaintySourceGenerators);
        }

        private HumanMonitoringAnalysisActionResult compute(
            ActionData data,
            ProgressState localProgress,
            HumanMonitoringAnalysisModuleSettings settings,
            bool isMcrAnalyis,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            // Create HBM concentration models
            var concentrationModelsBuilder = new HbmConcentrationModelBuilder();
            var concentrationModels = settings.NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit
                ? concentrationModelsBuilder.Create(
                    data.HbmSampleSubstanceCollections,
                    settings.NonDetectsHandlingMethod,
                    settings.LorReplacementFactor
                )
                : null;

            // Imputation of censored values
            var nonDetectsImputationCalculator = new HbmNonDetectsImputationCalculator(
                settings.NonDetectImputationMethod,
                settings.NonDetectsHandlingMethod,
                settings.LorReplacementFactor
            );

            var imputedNonDetectsSubstanceCollection = nonDetectsImputationCalculator
                .ImputeNonDetects(
                    data.HbmSampleSubstanceCollections,
                    settings.NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit ? concentrationModels : null,
                    factorialSet?.Contains(UncertaintySource.HbmNonDetectImputation) ?? false
                        ? RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.HbmNonDetectImputation].Seed, (int)RandomSource.HBM_CensoredValueImputation)
                        : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_CensoredValueImputation)
            );

            // Impute missing values
            var missingValueImputationCalculator = HbmMissingValueImputationCalculatorFactory
                .Create(_project.HumanMonitoringSettings.MissingValueImputationMethod);

            var imputedMissingValuesSubstanceCollection = missingValueImputationCalculator
                .ImputeMissingValues(
                    imputedNonDetectsSubstanceCollection,
                    settings.MissingValueCutOff,
                    factorialSet?.Contains(UncertaintySource.HbmMissingValueImputation) ?? false
                        ? RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.HbmMissingValueImputation].Seed, (int)RandomSource.HBM_MissingValueImputation)
                        : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_MissingValueImputation)
            );

            // Standardize blood concentrations (express soluble substances per lipid content)
            var standardisedSubstanceCollections = imputedMissingValuesSubstanceCollection;
            if (settings.StandardiseBlood) {
                var substancesExcludedFromLipidStandardisation = settings.StandardiseBloodExcludeSubstances ? settings.StandardiseBloodExcludedSubstancesSubset : new();
                var lipidContentCorrector = BloodCorrectionCalculatorFactory.Create(settings.StandardiseBloodMethod, substancesExcludedFromLipidStandardisation);
                standardisedSubstanceCollections = lipidContentCorrector
                    .ComputeResidueCorrection(
                        imputedMissingValuesSubstanceCollection
                    );
            }

            // Normalise by specific gravity or standardise by creatinine concentration
            if (settings.StandardiseUrine) {
                var substancesExcludedFromUrineStandardisation = settings.StandardiseUrineExcludeSubstances 
                    ? settings.StandardiseUrineExcludedSubstancesSubset : new();
                var urineCorrectorCalculator = UrineCorrectionCalculatorFactory
                    .Create(
                        settings.StandardiseUrineMethod,
                        settings.SpecificGravityConversionFactor,
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
            simulatedIndividualDays = IndividualDaysGenerator
                .ImputeBodyWeight(simulatedIndividualDays).ToList();

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

            if (settings.ApplyExposureBiomarkerConversions || settings.ApplyKineticConversions) {

                // Before conversion, filter on complete cases
                hbmIndividualDayCollections = GetCompleteCases(
                    hbmIndividualDayCollections,
                    settings.StandardiseBloodExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase),
                    settings.StandardiseUrineExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase)
                );

                // Store the day concentrations derived for the measured matrices
                result.HbmMeasuredMatrixIndividualDayCollections = hbmIndividualDayCollections;

                // Apply exposure biomarker conversion.
                if (settings.ApplyExposureBiomarkerConversions) {
                    var seed = factorialSet?.Contains(UncertaintySource.ExposureBiomarkerConversion) ?? false
                        ? RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.ExposureBiomarkerConversion].Seed, (int)RandomSource.HBM_ExposureBiomarkerConversion)
                        : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HBM_ExposureBiomarkerConversion);
                    var conversionCalculator = new ExposureBiomarkerConversionCalculator(data.ExposureBiomarkerConversionModels);
                    hbmIndividualDayCollections = conversionCalculator.Convert(
                        hbmIndividualDayCollections,
                        seed
                    );
                }

                if (settings.ApplyKineticConversions) {
                    var substances = data.ActiveSubstances ?? data.AllCompounds;

                    if (settings.HbmConvertToSingleTargetMatrix) {
                        // Kinetic conversions to a single target
                        hbmIndividualDayCollections = HbmSingleTargetExtrapolationCalculator
                            .Calculate(
                                hbmIndividualDayCollections,
                                data.KineticConversionFactorModels,
                                simulatedIndividualDays,
                                substances,
                                settings.TargetLevelType,
                                settings.TargetMatrix
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
                settings.StandardiseBloodExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase),
                settings.StandardiseUrineExcludedSubstancesSubset.ToHashSet(StringComparer.OrdinalIgnoreCase)
            );

            // Throw an exception if we all individual days were removed due to missing substance concentrations
            if (!individualDayCollections.SelectMany(c => c.HbmIndividualDayConcentrations).Any()) {
                throw new Exception("All HBM individual day records were removed for having non-imputed missing substance concentrations.");
            }

            // For chronic assessments, compute average individual concentrations
            List<HbmIndividualCollection> individualCollections = null;
            if (settings.ExposureType == ExposureType.Chronic) {
                var individualConcentrationsCalculator = new HbmIndividualConcentrationsCalculator();
                individualCollections = individualConcentrationsCalculator
                    .Calculate(
                        data.ActiveSubstances,
                        individualDayCollections
                    );
                if (result.HbmMeasuredMatrixIndividualDayCollections?.Any() ?? false) {
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
                    if (settings.ExposureType == ExposureType.Chronic) {
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
            if (_project.HumanMonitoringSettings.AnalyseMcr
                && data.ActiveSubstances.Count > 1
                && uncertaintySourceGenerators == null
                && isMcrAnalyis
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    data.ActiveSubstances,
                    data.ReferenceSubstance == null ? data.ActiveSubstances.ToDictionary(r => r, r => 1D) : data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    _project.AssessmentSettings.ExposureType,
                    _project.SubsetSettings.IsPerPerson,
                    _project.HumanMonitoringSettings.ExposureApproachType,
                    _project.MixtureSelectionSettings.TotalExposureCutOff,
                    _project.MixtureSelectionSettings.RatioCutOff
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
                .Where(r => r.Count() == result.Count())
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
            var summarizer = new HumanMonitoringAnalysisSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, HumanMonitoringAnalysisActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new HumanMonitoringAnalysisSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, HumanMonitoringAnalysisActionResult result) {
            updateSimulationData(data, result);
        }
    }
}

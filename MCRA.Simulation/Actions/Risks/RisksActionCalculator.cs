﻿using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.RiskPercentilesCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.Risks {

    [ActionType(ActionType.Risks)]
    public class RisksActionCalculator : ActionCalculatorBase<RisksActionResult> {
        private RisksModuleConfig ModuleConfig => (RisksModuleConfig)_moduleSettings;

        public RisksActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isTargetLevelExternal = ModuleConfig.TargetDoseLevelType == TargetLevelType.External;
            var isMonitoringConcentrations = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration;
            var isComputeFromModelledExposures = ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration;
            var isCumulative = ModuleConfig.MultipleSubstances && ModuleConfig.CumulativeRisk;
            var requiresRpfs = isCumulative && ModuleConfig.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = requiresRpfs;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = requiresRpfs;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = isComputeFromModelledExposures && isTargetLevelExternal;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = isComputeFromModelledExposures && isTargetLevelExternal;
            _actionInputRequirements[ActionType.TargetExposures].IsVisible = isComputeFromModelledExposures && !isTargetLevelExternal;
            _actionInputRequirements[ActionType.TargetExposures].IsRequired = isComputeFromModelledExposures && !isTargetLevelExternal;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsRequired = isMonitoringConcentrations;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsVisible = isMonitoringConcentrations;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new RisksSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new RisksSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override RisksActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            // Intra species random generator
            var intraSpeciesRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.RSK_DrawIntraSpeciesFactors));

            var result = ModuleConfig.ExposureType == ExposureType.Chronic ?
                compute<ITargetIndividualExposure>(ExposureType.Chronic, data, intraSpeciesRandomGenerator) :
                compute<ITargetIndividualDayExposure>(ExposureType.Acute, data, intraSpeciesRandomGenerator);

            if (ModuleConfig.McrAnalysis
                && ModuleConfig.IsCumulative && data.ActiveSubstances.Count > 1
                && (result.IndividualEffectsBySubstanceCollections?.Count > 0)
            ) {
                var riskMatrixBuilder = new ExposureMatrixBuilder(
                    data.ActiveSubstances,
                    data.ReferenceSubstance == null
                        ? data.ActiveSubstances.ToDictionary(r => r, r => 1D)
                        : data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    ModuleConfig.ExposureType,
                    false,
                    ModuleConfig.McrExposureApproachType,
                    ModuleConfig.McrCalculationTotalExposureCutOff,
                    ModuleConfig.McrCalculationRatioCutOff
                 );
                result.RiskMatrix = riskMatrixBuilder.Compute(
                    result.IndividualEffectsBySubstanceCollections,
                    ModuleConfig.RiskMetricCalculationType
                );
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.RiskMatrix);
            }

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(
            RisksActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (actionResult != null) {
                localProgress.Update("Summarizing risk results", 0);
                var summarizer = new RisksSummarizer(ModuleConfig);
                summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }

        protected override RisksActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Intra species random generator
            var intraSpeciesRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.RSK_DrawIntraSpeciesFactors));

            var result = ModuleConfig.ExposureType == ExposureType.Chronic ?
                compute<ITargetIndividualExposure>(ExposureType.Chronic, data, intraSpeciesRandomGenerator) :
                compute<ITargetIndividualDayExposure>(ExposureType.Acute, data, intraSpeciesRandomGenerator);

            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, RisksActionResult result) {
            data.CumulativeIndividualEffects = result.IndividualRisks;
        }

        protected override void updateSimulationDataUncertain(ActionData data, RisksActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void summarizeActionResultUncertain(
            UncertaintyFactorialSet factorialSet,
            RisksActionResult actionResult,
            ActionData data,
            SectionHeader header,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new RisksSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(_actionSettings, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, RisksActionResult result) {
            var outputWriter = new RisksOutputWriter();
            outputWriter.WriteOutputData(ModuleConfig, data, result, rawDataWriter);
        }

        protected override void writeOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, RisksActionResult result, int idBootstrap) {
            var outputWriter = new RisksOutputWriter();
            outputWriter.UpdateOutputData(ModuleConfig, rawDataWriter, data, result, idBootstrap);
        }

        protected override IActionComparisonData loadActionComparisonData(ICompiledDataManager compiledDataManager) {
            var result = new RisksActionComparisonData() {
                RiskModels = compiledDataManager.GetAllRiskModels()?.Values
            };
            return result;
        }

        public override void SummarizeComparison(ICollection<IActionComparisonData> comparisonData, SectionHeader header) {
            var models = comparisonData
                .Where(r => (r as RisksActionComparisonData).RiskModels?.Count > 0)
                .Select(r => {
                    var result = (r as RisksActionComparisonData).RiskModels.First();
                    result.Code = r.IdResultSet;
                    result.Name = r.NameResultSet;
                    return result;
                })
                .ToList();
            var summarizer = new RisksCombinedActionSummarizer();
            summarizer.Summarize(models, header);
        }

        private RisksActionResult compute<T>(
            ExposureType exposureType,
            ActionData data,
            IRandom intraSpeciesRandomGenerator
        ) where T : ITargetIndividualExposure {
            var result = new RisksActionResult();
            // Get hazard characterisation models targets
            var hazardTargets = data.HazardCharacterisationModelsCollections
                .Select(r => r.TargetUnit.Target)
                .ToList();

            var hazardTargetSubstanceTuples = data.HazardCharacterisationModelsCollections
                .SelectMany(
                    r => r.HazardCharacterisationModels.Keys,
                    (hc, c) => (Target: hc.TargetUnit.Target, Substance: c)
                )
                .ToHashSet();
            var hazardSubstances = hazardTargetSubstanceTuples.Select(r => r.Substance).ToHashSet();

            // Get exposures per target
            var exposuresCollections = getExposures<T>(data);
            var exposureTargets = exposuresCollections.Keys;
            var exposuresTargetSubstanceTuples = exposuresCollections
                .SelectMany(
                    r => r.Value.Exposures.SelectMany(i => i.Substances).Distinct(),
                    (e, c) => (Target: e.Key, Substance: c)
                )
                .ToHashSet();
            var exposureSubstances = exposuresTargetSubstanceTuples.Select(r => r.Substance).ToHashSet();

            // Missing hazard characterisations
            var missingHazardTargetTuples = exposuresTargetSubstanceTuples.Except(hazardTargetSubstanceTuples).ToList();
            var missingHazardSubstances = exposureSubstances.Except(hazardSubstances).ToList();
            var missingExposureTargetTuples = hazardTargetSubstanceTuples.Except(exposuresTargetSubstanceTuples).ToList();
            var missingExposureSubstances = hazardSubstances.Except(exposureSubstances).ToList();

            var riskTargets = hazardTargets.Intersect(exposureTargets).Distinct().ToList();

            // Duplicate risk substances
            var duplicateRiskSubstances = hazardTargetSubstanceTuples.Intersect(exposuresTargetSubstanceTuples)
                .GroupBy(r => r.Substance)
                .Where(r => r.Count() > 1)
                .Select(r => r.Key)
                .ToList();

            // Draw intra-species random number (TODO: refactor)
            foreach (var collection in exposuresCollections.Values) {
                if (collection.Exposures != null) {
                    for (int i = 0; i < collection.Exposures.Count; i++) {
                        var draw = intraSpeciesRandomGenerator.NextDouble();
                        collection.Exposures[i].IntraSpeciesDraw = draw;
                    }
                }
            }

            var riskCalculator = new RiskCalculator<T>(ModuleConfig.HealthEffectType);
            var targetUnits = new HashSet<TargetUnit>();

            // Single-substance
            if (data.ActiveSubstances.Count == 1) {
                var substance = data.ActiveSubstances.First();
                if (riskTargets.Count == 0) {
                    throw new Exception("Hazard characterisation target does not match exposures target.");
                } else if (riskTargets.Count > 1) {
                    throw new Exception("Single-substance risk calculation for multiple targets not implemented.");
                }
                var target = riskTargets.First();

                // Get exposures for target
                var (exposures, exposureUnit) = exposuresCollections[target];

                // Get hazard characterisations for target
                var hazardCharacterisationModelsCollection = data.HazardCharacterisationModelsCollections
                    .Single(c => c.TargetUnit.Target == target);
                var hazardCharacterisation = hazardCharacterisationModelsCollection.HazardCharacterisationModels[substance];
                result.ReferenceDose = hazardCharacterisation;
                var targetUnit = hazardCharacterisationModelsCollection.TargetUnit;
                targetUnits.Add(targetUnit);

                // Risks by substance
                var individualRiskRecords = riskCalculator
                    .ComputeSingleSubstance(
                        exposures,
                        exposureUnit,
                        hazardCharacterisation,
                        hazardCharacterisationModelsCollection.TargetUnit,
                        substance
                    );
                result.IndividualRisks = individualRiskRecords;
            } else {
                // Compute risks by substance
                if (!ModuleConfig.IsCumulative
                    || ModuleConfig.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios
                ) {
                    // Check for missing hazard characterisations
                    if (missingHazardTargetTuples.Any() && missingHazardSubstances.Any()) {
                        var missings = riskTargets.Count > 1
                            ? missingHazardTargetTuples.Select(r => $"[{r.Substance.Code}, {r.Target.Code}]").ToList()
                            : missingHazardTargetTuples.Select(r => $"[{r.Substance.Code}]").ToList();
                        var msg = "Missing hazard characterisations for "
                            + (missings.Count < 5
                                ? string.Join(", ", missings)
                                : riskTargets.Count > 1
                                    ? $"{missings.Count} target/substance combinations"
                                    : $"{missings.Count} substances");
                        throw new Exception(msg);
                    }

                    // Check for missing exposures
                    if (missingExposureTargetTuples.Any() && missingExposureSubstances.Any()) {
                        var missings = riskTargets.Count > 1
                            ? missingExposureTargetTuples.Select(r => $"[{r.Substance.Code}, {r.Target.Code}])").ToList()
                            : missingExposureTargetTuples.Select(r => $"[{r.Substance.Code}]").ToList();
                        var msg = "Missing exposures for "
                            + (missings.Count < 5
                                ? string.Join(", ", missings)
                                : riskTargets.Count > 1
                                    ? $"{missings.Count} target/substance combinations"
                                    : $"{missings.Count} substances");
                        if (riskTargets.Count == 1 && riskTargets.First() != ExposureTarget.DietaryExposureTarget) {
                            throw new Exception(msg);
                        }
                    }

                    result.IndividualEffectsBySubstanceCollections = [];
                    foreach (var target in riskTargets) {
                        // Get hazard characterisations for target
                        var hazardCharacterisationModelsCollection = data.HazardCharacterisationModelsCollections
                            .Single(c => c.TargetUnit.Target == target);
                        var hazardCharacterisations = hazardCharacterisationModelsCollection.HazardCharacterisationModels;
                        var targetUnit = hazardCharacterisationModelsCollection.TargetUnit;
                        targetUnits.Add(targetUnit);

                        // Risks by substance
                        var individualEffectsBySubstance = riskCalculator
                            .ComputeBySubstance(
                                exposuresCollections[target].Exposures,
                                exposuresCollections[target].Unit,
                                hazardCharacterisations,
                                hazardCharacterisationModelsCollection.TargetUnit,
                                hazardCharacterisations.Keys
                            );
                        result.IndividualEffectsBySubstanceCollections.Add(
                            (target, individualEffectsBySubstance)
                        );
                    }
                }

                // Cumulative risk
                if (ModuleConfig.IsCumulative) {
                    if (ModuleConfig.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {

                        // Risks as cumulative RPF weighted sum
                        var referenceSubstance = data.ReferenceSubstance;
                        if (exposuresCollections.Count > 1 || data.HazardCharacterisationModelsCollections.Count > 1) {
                            throw new Exception("Cannot compute cumulative risks: RPF weighting not implemented for multiple targets.");
                        }
                        if (data.ActiveSubstances.Count > 1
                            && (data.CorrectedRelativePotencyFactors == null || !data.CorrectedRelativePotencyFactors.Any())
                        ) {
                            throw new Exception("Cannot compute cumulative risks: no RPFs were set.");
                        }
                        if (data.ReferenceSubstance == null) {
                            throw new Exception("Cannot compute cumulative risks: reference substance not specified.");
                        }

                        // Get exposures
                        var individualExposures = exposuresCollections.Single().Value;
                        var exposuresTarget = exposuresCollections.Single().Key;

                        // Get hazard characterisation
                        var hazardCharacterisationModelsCollection = data.HazardCharacterisationModelsCollections.Single();
                        if (!hazardCharacterisationModelsCollection.HazardCharacterisationModels.TryGetValue(referenceSubstance, out var referenceDose)) {
                            throw new Exception($"For {referenceSubstance.Name} ({referenceSubstance.Code}) no hazard characterisation is available).");
                        }
                        var hazardCharacterisationUnit = hazardCharacterisationModelsCollection.TargetUnit;

                        if (exposuresTarget != hazardCharacterisationUnit.Target) {
                            throw new Exception($"Exposure target [{exposuresTarget.GetDisplayName()}] does not match with hazard target [{hazardCharacterisationUnit.Target.GetDisplayName()}].");
                        }

                        result.ReferenceDose = referenceDose;
                        targetUnits.Add(hazardCharacterisationUnit);

                        // Risks by substance
                        var target = riskTargets.FirstOrDefault();
                        var individualEffectsBySubstance = riskCalculator
                            .ComputeBySubstanceRpfWeighted(
                                individualExposures.Exposures,
                                individualExposures.Unit,
                                hazardCharacterisationUnit,
                                data.ActiveSubstances,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities,
                                referenceDose
                            );
                        result.IndividualEffectsBySubstanceCollections = [
                            (target, individualEffectsBySubstance)
                        ];

                        // Cumulative risks
                        var cumulativeIndividualRisks = riskCalculator
                            .ComputeRpfWeighted(
                                individualExposures.Exposures,
                                individualExposures.Unit,
                                referenceDose,
                                hazardCharacterisationUnit,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities,
                                data.ReferenceSubstance
                            );
                        result.IndividualRisks = cumulativeIndividualRisks;
                    } else if (ModuleConfig.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {

                        // Check for multiple risk targets for the same substance
                        if (duplicateRiskSubstances.Any()) {
                            var msg = "Multiple target matrices for "
                                + (duplicateRiskSubstances.Count < 5
                                    ? string.Join(", ", duplicateRiskSubstances.Select(r => $"{r.Name} ({r.Code})"))
                                    : $"{duplicateRiskSubstances.Count} substances")
                                    + ".";
                            throw new Exception(msg);
                        }

                        // Risks as sum of ratios
                        var individualEffectsByTarget = result.IndividualEffectsBySubstanceCollections
                            .Select(r => {
                                var cumulativeIndividualRisks = riskCalculator
                                    .ComputeSumOfRatios(
                                        r.IndividualEffects,
                                        data.MembershipProbabilities
                                    );
                                return (r.Target, cumulativeIndividualRisks);
                            })
                            .ToList();

                        var cumulativeIndividualRisks = individualEffectsByTarget
                            .SelectMany(r => r.cumulativeIndividualRisks, (r, i) => (r.Target, i))
                            .GroupBy(r => r.i.SimulatedIndividualId)
                            .Select(r => new IndividualEffect() {
                                SimulatedIndividualId = r.Key,
                                SimulatedIndividual = r.First().i.SimulatedIndividual,
                                ExposureHazardRatio = r.Sum(c => c.i.ExposureHazardRatio),
                                HazardExposureRatio = 1 / r.Sum(c => 1 / c.i.HazardExposureRatio),
                                IsPositive = r.Any(c => c.i.IsPositive)
                            })
                            .ToList();

                        var targetUnit = targetUnits.Count == 1 ? targetUnits.First() : null;
                        result.IndividualRisks = cumulativeIndividualRisks;
                        targetUnits.Add(targetUnit);
                    }
                }
            }
            result.TargetUnits = targetUnits.ToList();

            // Risks by food and risks by substance
            if (ModuleConfig.TargetDoseLevelType == TargetLevelType.External
                && ModuleConfig.CalculateRisksByFood
                && (data.ActiveSubstances.Count > 1 && ModuleConfig.IsCumulative)
                && ModuleConfig.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                computeRisksByFoodAndSubstance(exposureType, data, ModuleConfig, result, exposuresCollections);
            }

            // Risk percentiles
            if (!ModuleConfig.MultipleSubstances || ModuleConfig.IsCumulative) {
                var percentilesCalculator = new RiskDistributionPercentilesCalculator(
                    ModuleConfig.HealthEffectType,
                    ModuleConfig.RiskMetricType,
                    ModuleConfig.RiskPercentiles,
                    ModuleConfig.IsInverseDistribution
                );
                result.RiskPercentiles = percentilesCalculator.Compute(result.IndividualRisks);
            }
            result.ExposureTargets = riskTargets;
            return result;
        }

        private static void computeRisksByFoodAndSubstance<T>(
            ExposureType exposureType,
            ActionData data,
            RisksModuleConfig config,
            RisksActionResult result,
            Dictionary<ExposureTarget, (List<T> TargetExposures, TargetUnit Targetunit)> exposuresCollections
        ) where T : ITargetIndividualExposure {
            var individualExposures = exposuresCollections.Single().Value.TargetExposures;
            var hazardCharacterisationModelsCollection = data.HazardCharacterisationModelsCollections.Single();
            var hazardCharacterisationModels = hazardCharacterisationModelsCollection.HazardCharacterisationModels;
            var hazardCharacterisationUnit = hazardCharacterisationModelsCollection.TargetUnit;

            if (exposureType == ExposureType.Chronic) {
                var risksByFoodCalculator = new RisksByFoodCalculator(config.HealthEffectType);
                var dietaryIndividualTargetExposures = individualExposures
                    .Cast<DietaryIndividualTargetExposureWrapper>().ToList();
                result.IndividualEffectsByModelledFood = risksByFoodCalculator
                    .ComputeByModelledFood(
                        dietaryIndividualTargetExposures,
                        exposuresCollections.Single().Value.Targetunit,
                        hazardCharacterisationModels,
                        hazardCharacterisationUnit,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ModelledFoods,
                        data.ReferenceSubstance
                    );

                var risksBySubstanceCalculator = new RisksByModelledFoodSubstanceCalculator(config.HealthEffectType);
                result.IndividualEffectsByModelledFoodSubstance = risksBySubstanceCalculator
                    .ComputeByModelledFoodSubstance(
                        dietaryIndividualTargetExposures,
                        exposuresCollections.Single().Value.Targetunit,
                        hazardCharacterisationModels,
                        hazardCharacterisationUnit,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ModelledFoods,
                        data.ReferenceSubstance
                    );
            } else {
                var risksByFoodCalculator = new RisksByFoodCalculator(config.HealthEffectType);
                var dietaryIndividualDayTargetExposures = individualExposures.Cast<DietaryIndividualDayTargetExposureWrapper>().ToList();
                result.IndividualEffectsByModelledFood = risksByFoodCalculator
                    .ComputeByModelledFood(
                        dietaryIndividualDayTargetExposures,
                        exposuresCollections.Single().Value.Targetunit,
                        hazardCharacterisationModels,
                        hazardCharacterisationUnit,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ModelledFoods,
                        data.ReferenceSubstance
                    );

                var risksBySubstanceCalculator = new RisksByModelledFoodSubstanceCalculator(config.HealthEffectType);
                result.IndividualEffectsByModelledFoodSubstance = risksBySubstanceCalculator
                    .ComputeByModelledFoodSubstance(
                        dietaryIndividualDayTargetExposures,
                        exposuresCollections.Single().Value.Targetunit,
                        hazardCharacterisationModels,
                        hazardCharacterisationUnit,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ModelledFoods,
                        data.ReferenceSubstance
                    );
            }
        }

        private Dictionary<ExposureTarget, (List<T> Exposures, TargetUnit Unit)> getExposures<T>(
            ActionData data
        ) where T : ITargetIndividualExposure {
            var result = new Dictionary<ExposureTarget, (List<T>, TargetUnit)>();
            if (ModuleConfig.ExposureCalculationMethod == ExposureCalculationMethod.ModelledConcentration) {
                if (ModuleConfig.TargetDoseLevelType == TargetLevelType.External) {
                    // From dietary
                    if (ModuleConfig.ExposureType == ExposureType.Chronic) {
                        // chronic
                        var dietaryIndividualTargetExposures = data.DietaryIndividualDayIntakes
                            .AsParallel()
                            .GroupBy(c => c.SimulatedIndividual.Id)
                            .Select(c => new DietaryIndividualTargetExposureWrapper(c.ToList(), data.DietaryExposureUnit.ExposureUnit))
                            .OrderBy(r => r.SimulatedIndividual.Id)
                            .ToList();
                        result.Add(
                            ExposureTarget.DietaryExposureTarget,
                            (dietaryIndividualTargetExposures.Cast<T>().ToList(), data.DietaryExposureUnit)
                        );
                    } else {
                        // acute
                        var dietaryIndividualDayTargetExposures = data.DietaryIndividualDayIntakes
                            .AsParallel()
                            .Select(c => new DietaryIndividualDayTargetExposureWrapper(c, data.DietaryExposureUnit.ExposureUnit))
                            .OrderBy(r => r.SimulatedIndividualDayId)
                            .ToList();
                        result.Add(
                            ExposureTarget.DietaryExposureTarget,
                            (dietaryIndividualDayTargetExposures.Cast<T>().ToList(), data.DietaryExposureUnit)
                        );
                    }
                } else {
                    // From aggregate/internal exposures
                    if (ModuleConfig.ExposureType == ExposureType.Chronic) {
                        // chronic
                        var internalTargetExposures = data.AggregateIndividualExposures
                            .AsParallel()
                            .Select(c => new AggregateIndividualTargetExposureWrapper(c, data.TargetExposureUnit))
                            .OrderBy(r => r.SimulatedIndividual.Id)
                            .ToList();
                        result.Add(
                            data.TargetExposureUnit.Target,
                            (internalTargetExposures.Cast<T>().ToList(), data.TargetExposureUnit)
                        );
                    } else {
                        // acute
                        var internalTargetExposures = data.AggregateIndividualDayExposures
                            .AsParallel()
                            .Select(c => new AggregateIndividualDayTargetExposureWrapper(c, data.TargetExposureUnit))
                            .OrderBy(r => r.SimulatedIndividualDayId)
                            .ToList();
                        result.Add(
                            data.TargetExposureUnit.Target,
                            (internalTargetExposures.Cast<T>().ToList(), data.TargetExposureUnit)
                        );
                    }
                }
            } else {
                // From HBM
                result = ModuleConfig.ExposureType == ExposureType.Chronic
                    ? data.HbmIndividualCollections
                        .ToDictionary(
                            r => r.TargetUnit.Target,
                            r => (r.HbmIndividualConcentrations.Cast<T>().ToList(), r.TargetUnit)
                        )
                    : data.HbmIndividualDayCollections
                        .ToDictionary(
                            r => r.Target,
                            r => (r.HbmIndividualDayConcentrations.Cast<T>().ToList(), r.TargetUnit)
                    );
            }
            return result;
        }
    }
}

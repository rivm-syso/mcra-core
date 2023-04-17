using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
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

        public RisksActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isTargetLevelInternal = _project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal;
            var isMonitoringConcentrations = _project.AssessmentSettings.InternalConcentrationType == InternalConcentrationType.MonitoringConcentration;
            var isCumulative = _project.AssessmentSettings.MultipleSubstances
                && _project.EffectModelSettings.CumulativeRisk;
            var requiresRpfs = isCumulative && _project.EffectModelSettings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = requiresRpfs;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = requiresRpfs;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = !isTargetLevelInternal;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = !isTargetLevelInternal;
            _actionInputRequirements[ActionType.TargetExposures].IsVisible = isTargetLevelInternal && !isMonitoringConcentrations;
            _actionInputRequirements[ActionType.TargetExposures].IsRequired = isTargetLevelInternal && !isMonitoringConcentrations;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsRequired = isMonitoringConcentrations && isTargetLevelInternal;
            _actionInputRequirements[ActionType.HumanMonitoringAnalysis].IsVisible = isMonitoringConcentrations && isTargetLevelInternal;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new RisksSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new RisksSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override RisksActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new RisksModuleSettings(_project);

            // Intra species random generator
            var intraSpeciesRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.RSK_DrawIntraSpeciesFactors));

            var result = _project.AssessmentSettings.ExposureType == ExposureType.Chronic ?
                compute<ITargetIndividualExposure>(ExposureType.Chronic, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations) :
                compute<ITargetIndividualDayExposure>(ExposureType.Acute, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations);

            localProgress.Update(100);

            return result;
        }

        protected override void summarizeActionResult(RisksActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (actionResult != null) {
                localProgress.Update("Summarizing risk results", 0);
                var summarizer = new RisksSummarizer();
                summarizer.Summarize(_project, actionResult, data, header, order);
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

            var settings = new RisksModuleSettings(_project);

            // Intra species random generator
            var intraSpeciesRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.RSK_DrawIntraSpeciesFactors));

            var result = settings.ExposureType == ExposureType.Chronic ?
                compute<ITargetIndividualExposure>(ExposureType.Chronic, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations) :
                compute<ITargetIndividualDayExposure>(ExposureType.Acute, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations);

            localProgress.Update(100);

            return result;
        }

        protected override void updateSimulationData(ActionData data, RisksActionResult result) {
            data.CumulativeIndividualEffects = result.IndividualEffects;
        }

        protected override void updateSimulationDataUncertain(ActionData data, RisksActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, RisksActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new RisksSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, RisksActionResult result) {
            var outputWriter = new RisksOutputWriter();
            outputWriter.WriteOutputData(_project, data, result, rawDataWriter);
        }

        protected override void writeOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, RisksActionResult result, int idBootstrap) {
            var outputWriter = new RisksOutputWriter();
            outputWriter.UpdateOutputData(_project, rawDataWriter, data, result, idBootstrap);
        }

        protected override IActionComparisonData loadActionComparisonData(ICompiledDataManager compiledDataManager) {
            var result = new RisksActionComparisonData() {
                RiskModels = compiledDataManager.GetAllRiskModels()?.Values
            };
            return result;
        }

        public override void SummarizeComparison(ICollection<IActionComparisonData> comparisonData, SectionHeader header) {
            var models = comparisonData
                .Where(r => (r as RisksActionComparisonData).RiskModels?.Any() ?? false)
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
            RisksModuleSettings settings,
            IRandom intraSpeciesRandomGenerator,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations
        ) where T : ITargetIndividualExposure {
            var result = new RisksActionResult();

            if (data.ActiveSubstances.Count == 1) {
                // Only one substance, automatically the reference substance
                var referenceSubstance = data.ActiveSubstances.First();
                if (!data.HazardCharacterisations.TryGetValue(referenceSubstance, out var referenceDose)) {
                    throw new Exception($"No hazard characterisation available for substance {referenceSubstance.Name} ({referenceSubstance.Code}).");
                }
            } else if (settings.IsCumulative && settings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                // In case of RPF-weighing
                var referenceSubstance = data.ReferenceSubstance;
                if (!data.HazardCharacterisations.TryGetValue(referenceSubstance, out var referenceDose)) {
                    throw new Exception($"No hazard characterisation available for the reference substance {referenceSubstance.Name} ({referenceSubstance.Code}).");
                }
                result.ReferenceDose = referenceDose;
            }

            // Draw intra-species random number (TODO: refactor)
            var exposures = getExposures<T>(exposureType, data, settings);
            for (int i = 0; i < exposures.Count; i++) {
                var draw = intraSpeciesRandomGenerator.NextDouble();
                exposures[i].IntraSpeciesDraw = draw;
            }

            var riskCalculator = new RiskCalculator<T>();

            var isMissingHazardCharacterisations = data.ActiveSubstances
                .Any(r => !data.HazardCharacterisations.ContainsKey(r));

            // Risks by substance
            if (!isMissingHazardCharacterisations) {
                // If we can, comput individual effects by substance
                result.IndividualEffectsBySubstance = riskCalculator.ComputeBySubstance(
                    exposures,
                    hazardCharacterisations,
                    data.ActiveSubstances,
                    data.HazardCharacterisationsUnit,
                    settings.HealthEffectType,
                    settings.IsPerPerson
                );
            } else if (!settings.IsCumulative || settings.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                // If not cumulative or cumulative method is sum of ratios, then we need individual effects by substance
                throw new Exception("Cannot compute risks per substance: not all substances have a hazard characterisation.");
            }

            if (settings.IsCumulative && settings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                // Risks as cumulative RPF weighted sum
                if (data.CorrectedRelativePotencyFactors == null || !data.CorrectedRelativePotencyFactors.Any()) {
                    throw new Exception("Cannot compute cumulative risks: no RPFs were set.");
                }
                if (data.ReferenceSubstance == null) {
                    throw new Exception("Cannot compute cumulative risks: reference substance not specified.");
                }
                result.IndividualEffects = riskCalculator.ComputeCumulative(
                    exposures,
                    hazardCharacterisations,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ReferenceSubstance,
                    data.HazardCharacterisationsUnit,
                    settings.HealthEffectType,
                    settings.IsPerPerson
                );
            } else if (settings.IsCumulative && settings.RiskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                // Risks as sum of ratios
                result.IndividualEffects = riskCalculator.ComputeSumOfRatios(
                    result.IndividualEffectsBySubstance,
                    data.MembershipProbabilities,
                    settings.HealthEffectType);
            }

            // Risks by food and risks by substance
            if (settings.IsCumulative
                && settings.CalculateRisksByFood
                && settings.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted
            ) {
                if (exposureType == ExposureType.Chronic) {
                    var risksByFoodCalculator = new RisksByFoodCalculator();
                    var dietaryIndividualTargetExposures = exposures.Cast<DietaryIndividualTargetExposureWrapper>().ToList();
                    result.IndividualEffectsByModelledFood = risksByFoodCalculator.ComputeByModelledFood(
                        dietaryIndividualTargetExposures,
                        hazardCharacterisations,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ReferenceSubstance,
                        data.HazardCharacterisationsUnit,
                        settings.IsPerPerson
                    );

                    var risksBySubstanceCalculator = new RisksByModelledFoodSubstanceCalculator();
                    result.IndividualEffectsByModelledFoodSubstance = risksBySubstanceCalculator.ComputeByModelledFoodSubstance(
                        dietaryIndividualTargetExposures,
                        hazardCharacterisations,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ReferenceSubstance,
                        data.HazardCharacterisationsUnit,
                        settings.IsPerPerson
                    );
                } else {
                    var risksByFoodCalculator = new RisksByFoodCalculator();
                    var dietaryIndividualDayTargetExposures = exposures.Cast<DietaryIndividualDayTargetExposureWrapper>().ToList();
                    result.IndividualEffectsByModelledFood = risksByFoodCalculator.ComputeByModelledFood(
                        dietaryIndividualDayTargetExposures,
                        hazardCharacterisations,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ReferenceSubstance,
                        data.HazardCharacterisationsUnit,
                        settings.IsPerPerson
                    );

                    var risksBySubstanceCalculator = new RisksByModelledFoodSubstanceCalculator();
                    result.IndividualEffectsByModelledFoodSubstance = risksBySubstanceCalculator.ComputeByModelledFoodSubstance(
                        dietaryIndividualDayTargetExposures,
                        hazardCharacterisations,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ReferenceSubstance,
                        data.HazardCharacterisationsUnit,
                        settings.IsPerPerson
                    );
                }
            }

            // Risk percentiles
            if (!settings.IsMultipleSubstances || settings.IsCumulative) {
                var percentilesCalculator = new RiskDistributionPercentilesCalculator(
                    settings.HealthEffectType,
                    settings.RiskMetricType,
                    settings.RiskPercentiles,
                    settings.UseInverseDistribution
                );
                result.RiskPercentiles = percentilesCalculator
                    .Compute(
                        settings.IsCumulative
                            ? result.IndividualEffects
                            : result.IndividualEffectsBySubstance.First().Value
                    );
            }

            return result;
        }

        private List<T> getExposures<T>(
            ExposureType exposureType,
            ActionData data,
            RisksModuleSettings settings
        ) where T : ITargetIndividualExposure {
            List<T> exposures = null;
            if (settings.TargetDoseLevelType == TargetLevelType.External) {
                if (exposureType == ExposureType.Chronic) {
                    // chronic
                    var dietaryIndividualTargetExposures = data.DietaryIndividualDayIntakes
                          .AsParallel()
                          .GroupBy(c => c.SimulatedIndividualId)
                          .Select(c => new DietaryIndividualTargetExposureWrapper(c.ToList()))
                          .OrderBy(r => r.SimulatedIndividualId)
                          .ToList();
                    exposures = dietaryIndividualTargetExposures.Cast<T>().ToList();
                } else {
                    // acute
                    var dietaryIndividualDayTargetExposures = data.DietaryIndividualDayIntakes
                             .AsParallel()
                             .Select(c => new DietaryIndividualDayTargetExposureWrapper(c))
                             .OrderBy(r => r.SimulatedIndividualDayId)
                             .ToList();
                    exposures = dietaryIndividualDayTargetExposures.Cast<T>().ToList();
                }
            } else {
                // Internal
                if (settings.InternalConcentrationType == InternalConcentrationType.ModelledConcentration) {
                    exposures = exposureType == ExposureType.Chronic
                              ? data.AggregateIndividualExposures.Cast<T>().ToList()
                              : data.AggregateIndividualDayExposures.Cast<T>().ToList();
                } else {
                    exposures = exposureType == ExposureType.Chronic
                              ? data.HbmIndividualConcentrations.Cast<T>().ToList()
                              : data.HbmIndividualDayConcentrations.Cast<T>().ToList();
                }
            }
            return exposures;
        }
    }
}

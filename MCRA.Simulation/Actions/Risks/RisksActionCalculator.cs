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
                    getIndividualEffects<ITargetIndividualExposure>(ExposureType.Chronic, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations) :
                    getIndividualEffects<ITargetIndividualDayExposure>(ExposureType.Acute, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations);

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
                    getIndividualEffects<ITargetIndividualExposure>(ExposureType.Chronic, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations) :
                    getIndividualEffects<ITargetIndividualDayExposure>(ExposureType.Acute, data, settings, intraSpeciesRandomGenerator, data.HazardCharacterisations);

            localProgress.Update(100);

            return result;
        }

        private RisksActionResult getIndividualEffects<T>(
            ExposureType exposureType,
            ActionData data,
            RisksModuleSettings settings,
            IRandom intraSpeciesRandomGenerator,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations
        ) where T : ITargetIndividualExposure {
            var isMissingHazardCharacterisations = data.ActiveSubstances
                .Any(r => !data.HazardCharacterisations.ContainsKey(r));
            if (isMissingHazardCharacterisations && !data.HazardCharacterisations.ContainsKey(data.ReferenceCompound)) {
                throw new Exception($"No hazard characterisation for the reference substance {data.ReferenceCompound.Code} available in table HazardCharactersiations");
            }

            var result = new RisksActionResult();
            var referenceSubstance = data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : data.ReferenceCompound;
            if (referenceSubstance != null && data.HazardCharacterisations.TryGetValue(referenceSubstance, out var referenceDose)) {
                result.ReferenceDose = referenceDose;
            }

            List<T> exposures = GetExposures<T>(exposureType, data, settings);
            for (int i = 0; i < exposures.Count; i++) {
                var draw = intraSpeciesRandomGenerator.NextDouble();
                exposures[i].IntraSpeciesDraw = draw;
            }

            var riskCalculator = new RiskCalculator<T>();

            // Cumulative
            if (settings.IsCumulative) {
                if (data.CorrectedRelativePotencyFactors == null || !data.CorrectedRelativePotencyFactors.Any()) {
                    throw new Exception("Cannot compute cumulative risks: no RPFs were set.");
                }
                if (data.ReferenceCompound == null) {
                    throw new Exception("Cannot compute cumulative risks: reference substance not specified.");
                }

                result.CumulativeIndividualEffects = riskCalculator.ComputeCumulative(
                    exposures,
                    hazardCharacterisations,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    data.ReferenceCompound,
                    data.HazardCharacterisationsUnit,
                    settings.IsPerPerson
                );
            }

            // Risks by substance
            if (!isMissingHazardCharacterisations) {
                result.IndividualEffectsBySubstance = riskCalculator.ComputeBySubstance(
                    exposures,
                    hazardCharacterisations,
                    data.ActiveSubstances,
                    data.HazardCharacterisationsUnit,
                    settings.IsPerPerson
                );
            }

            if (settings.IsCumulative
                && settings.CalculateRisksByFood 
                && !isMissingHazardCharacterisations
            ) {
                if (exposureType == ExposureType.Chronic) {
                    var risksByFoodCalculator = new RisksByFoodCalculator();
                    var dietaryIndividualTargetExposures = exposures.Cast<DietaryIndividualTargetExposureWrapper>().ToList();
                    result.IndividualEffectsByModelledFood = risksByFoodCalculator.ComputeByModelledFood(
                        dietaryIndividualTargetExposures,
                        hazardCharacterisations,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ReferenceCompound,
                        data.HazardCharacterisationsUnit,
                        settings.IsPerPerson
                    );

                    var risksBySubstanceCalculator = new RisksByModelledFoodSubstanceCalculator();
                    result.IndividualEffectsByModelledFoodSubstance = risksBySubstanceCalculator.ComputeByModelledFoodSubstance(
                        dietaryIndividualTargetExposures,
                        hazardCharacterisations,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ReferenceCompound,
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
                        data.ReferenceCompound,
                        data.HazardCharacterisationsUnit,
                        settings.IsPerPerson
                    );

                    var risksBySubstanceCalculator = new RisksByModelledFoodSubstanceCalculator();
                    result.IndividualEffectsByModelledFoodSubstance = risksBySubstanceCalculator.ComputeByModelledFoodSubstance(
                        dietaryIndividualDayTargetExposures,
                        hazardCharacterisations,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.ReferenceCompound,
                        data.HazardCharacterisationsUnit,
                        settings.IsPerPerson
                    );
                }
            }

            return result;
        }

        private List<T> GetExposures<T>(
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
                    exposures = data.AggregateIndividualExposures.Cast<T>().ToList();
                } else {
                    exposures = exposureType == ExposureType.Chronic ? data.HbmIndividualConcentrations.Cast<T>().ToList()
                                                                     : data.HbmIndividualDayConcentrations.Cast<T>().ToList();
                }
            }
            return exposures;
        }

        protected override void updateSimulationData(ActionData data, RisksActionResult result) {
            data.CumulativeIndividualEffects = result.CumulativeIndividualEffects;
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
    }
}

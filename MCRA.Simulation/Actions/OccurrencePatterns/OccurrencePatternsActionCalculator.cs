using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Management;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.OccurrencePatterns {

    [ActionType(ActionType.OccurrencePatterns)]
    public class OccurrencePatternsActionCalculator : ActionCalculatorBase<OccurrencePatternsActionResult> {

        public OccurrencePatternsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isScaleUp = _project.AgriculturalUseSettings.ScaleUpOccurencePatterns;
            var isCumulative = _project.AssessmentSettings.MultipleSubstances && _project.AssessmentSettings.Cumulative;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = false;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = isCumulative;
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsVisible = isScaleUp && _project.AgriculturalUseSettings.RestrictOccurencePatternScalingToAuthorisedUses;
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsRequired = isScaleUp && _project.AgriculturalUseSettings.RestrictOccurencePatternScalingToAuthorisedUses;
            _actionDataLinkRequirements[ScopingType.OccurrencePatterns][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.OccurrencePatternsHasCompounds][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new OccurrencePatternsSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new OccurrencePatternsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var agriculturalUses = subsetManager.AllOccurrencePatterns;
            var marginalAgriculturalUsesCalculator = new MarginalOccurrencePatternsCalculator();
            data.MarginalOccurrencePatterns = marginalAgriculturalUsesCalculator.ComputeMarginalOccurrencePatterns(data.AllFoods, agriculturalUses, data.SampleOriginInfos);
            localProgress.Update(100);
        }

        protected override OccurrencePatternsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var settings = new OccurrencePatternsFromFindingsCalculatorSettings(_project.AgriculturalUseSettings);

            var agriculturalUsesFindingsCalculator = new OccurrencePatternsFromFindingsCalculator(settings);
            //Hit summarizer settings, is needed
            var foods = data.ActiveSubstanceSampleCollections.Keys.OrderBy(r => r.Code).ToList();            
            var agriculturalUses = agriculturalUsesFindingsCalculator
                .Compute(
                    foods,
                    data.ActiveSubstanceSampleCollections, 
                    progressReport
                );

            var result = new OccurrencePatternsActionResult() {
                OccurrencePatterns = agriculturalUses
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, OccurrencePatternsActionResult result) {
            if (result != null) {
                data.MarginalOccurrencePatterns = result.OccurrencePatterns
                    .GroupBy(r => r.Food)
                    .ToDictionary(r => r.Key, r => r.ToList());
            }
        }

        protected override OccurrencePatternsActionResult runUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewCompositeState(100);
            if (factorialSet.Contains(UncertaintySource.Concentrations) && _project.UncertaintyAnalysisSettings.RecomputeOccurrencePatterns) {
                return run(data, progressReport);
            }
            localProgress.MarkCompleted();
            return null;
        }

        protected override void updateSimulationDataUncertain(ActionData data, OccurrencePatternsActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void summarizeActionResult(OccurrencePatternsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new OccurrencePatternsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, OccurrencePatternsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new OccurrencePatternsSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }
    }
}

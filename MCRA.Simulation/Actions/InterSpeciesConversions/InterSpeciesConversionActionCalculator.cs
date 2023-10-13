using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.InterSpeciesConversions {

    [ActionType(ActionType.InterSpeciesConversions)]
    public sealed class InterSpeciesConversionsActionCalculator : ActionCalculatorBase<IInterSpeciesConversionActionResult> {

        public InterSpeciesConversionsActionCalculator(ProjectDto project) : base(project) {
            var showActiveSubstances = _project.AssessmentSettings.MultipleSubstances
                && !_project.EffectSettings.RestrictToAvailableHazardCharacterisations;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.InterSpeciesModelParameters][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.InterSpeciesModelParameters][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void verify() {
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = base.GetRandomSources();
            if (_project.UncertaintyAnalysisSettings.ReSampleInterspecies) {
                result.Add(UncertaintySource.InterSpecies);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new InterSpeciesConversionsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadDefaultData(ActionData data) {
            var settings = new InterSpeciesFactorModelBuilderSettings(_project.RisksSettings);
            var interSpeciesFactorModelsBuilder = new InterSpeciesFactorModelsBuilder(settings);
            var interSpeciesConversionModels = interSpeciesFactorModelsBuilder.Create(null);
            data.InterSpeciesFactorModels = interSpeciesConversionModels;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var interSpeciesFactors = subsetManager.AllInterSpeciesFactors;
            var settings = new InterSpeciesFactorModelBuilderSettings(_project.RisksSettings);
            var interSpeciesFactorModelsBuilder = new InterSpeciesFactorModelsBuilder(settings);
            var interSpeciesConversionModels = interSpeciesFactorModelsBuilder.Create(interSpeciesFactors);
            data.InterSpeciesFactors = interSpeciesFactors;
            data.InterSpeciesFactorModels = interSpeciesConversionModels;
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IInterSpeciesConversionActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new InterSpeciesConversionSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (_project.ActionType == ActionType.InterSpeciesConversions || _project.EffectSettings.UseInterSpeciesConversionFactors) {
                if (factorialSet.Contains(UncertaintySource.InterSpecies)) {
                    var draw = uncertaintySourceGenerators[UncertaintySource.InterSpecies].NextDouble();
                    foreach (var model in data.InterSpeciesFactorModels.Values) {
                        model.ResampleUncertainty(draw);
                    }
                }
            }
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, IInterSpeciesConversionActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new InterSpeciesConversionSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }
    }
}

using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FocalFoodConcentrations {

    [ActionType(ActionType.FocalFoodConcentrations)]
    public sealed class FocalFoodConcentrationsActionCalculator : ActionCalculatorBase<IFocalFoodConcentrationsActionResult> {

        public FocalFoodConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            // Selection requirements
            _actionDataSelectionRequirements[ScopingType.FocalFoodAnalyticalMethods].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.FocalFoodSamples].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.FocalFoodSampleAnalyses].AllowEmptyScope = true;

            // Data link requirements
            _actionDataLinkRequirements[ScopingType.FocalFoodSamples][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FocalFoodConcentrationsPerSample][ScopingType.FocalFoodSampleAnalyses].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FocalFoodConcentrationsPerSample][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FocalFoodAnalyticalMethodCompounds][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new FocalFoodConcentrationsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var focalCommodityFoods = subsetManager.SelectedFocalCommodityFoods;
            var focalCommoditySubstances = subsetManager.SelectedFocalCommoditySubstances;
            var focalCommoditySamples = subsetManager.AllFocalCommoditySamples
                .Where(r => focalCommodityFoods.Contains(r.Food))
                .ToList();

            if (!focalCommoditySubstances.Any()) {
                focalCommoditySubstances =
                        focalCommoditySamples
                            .SelectMany(s => s.SampleAnalyses)
                            .SelectMany(s => s.AnalyticalMethod != null ? s.AnalyticalMethod.AnalyticalMethodCompounds.Keys : s.Concentrations.Keys)
                            .ToHashSet();
            }

            //Set concentration unit
            data.ConcentrationUnit = focalCommoditySubstances.Count == 1
                ? focalCommoditySubstances.First().ConcentrationUnit
                : ConcentrationUnit.mgPerKg;

            data.FocalCommoditySamples = focalCommoditySamples;
            data.FocalCommoditySubstanceSampleCollections = SampleCompoundCollectionsBuilder
                .Create(
                    focalCommodityFoods,
                    focalCommoditySubstances,
                    focalCommoditySamples,
                    data.ConcentrationUnit,
                    null,
                    progressState
                ).Values;
        }

        protected override void summarizeActionResult(IFocalFoodConcentrationsActionResult result, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new FocalFoodConcentrationsSummarizer();
            summarizer.Summarize(_project, result, data, header, order);
            localProgress.Update(100);
        }
    }
}

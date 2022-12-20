using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Wrappers.UnitVariability;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using System.Linq;

namespace MCRA.Simulation.Actions.UnitVariabilityFactors {

    [ActionType(ActionType.UnitVariabilityFactors)]
    public class UnitVariabilityFactorsActionCalculator : ActionCalculatorBase<IUnitVariabilityFactorsActionResult> {

        public UnitVariabilityFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.UnitVariabilityFactors][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.UnitVariabilityFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.UnitVariabilityFactors][ScopingType.ProcessingTypes].AlertTypeMissingData = AlertType.Notification;
            _actionDataSelectionRequirements[ScopingType.IestiSpecialCases].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.IestiSpecialCases][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.IestiSpecialCases][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var allUnitVariabilityFactors = subsetManager.GetAllUnitVariabilityFactors();
            data.UnitVariabilityDictionary = allUnitVariabilityFactors
                .GroupBy(uv => uv.Food)
                .Select(g => new FoodUnitVariabilityInfo(g.Key, g.ToList()))
                .ToDictionary(r => r.Food);
            data.IestiSpecialCases = subsetManager.GetAllIestiSpecialCases();
        }

        protected override void summarizeActionResult(IUnitVariabilityFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.UnitVariabilityDictionary != null) {
                var summarizer = new UnitVariabilityFactorsSummarizer();
                summarizer.Summarize(_project, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }
    }
}

using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.BaselineBodIndicators {

    [ActionType(ActionType.BaselineBodIndicators)]
    public class BaselineBodIndicatorsActionCalculator : ActionCalculatorBase<IBaselineBodIndicatorsActionResult> {

        public BaselineBodIndicatorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.BaselineBodIndicators][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {

            var baselineBodIndicators = subsetManager.AllBaselineBodIndicators;
            var list = new List<BaselineBodIndicator>();
            foreach (var bbi in baselineBodIndicators) {
                var record = new BaselineBodIndicator() {
                    Population = bbi.Population,
                    Effect = bbi.Effect,
                    BodIndicator = bbi.BodIndicator,
                    Value = bbi.Value
                };
                list.Add(record);
            }
            data.BaselineBodIndicators = list;
        }

        protected override void summarizeActionResult(IBaselineBodIndicatorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new BaselineBodIndicatorsSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}

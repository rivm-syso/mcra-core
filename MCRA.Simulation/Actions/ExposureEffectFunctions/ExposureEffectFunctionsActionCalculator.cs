using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ExposureEffectFunctions {

    [ActionType(ActionType.ExposureEffectFunctions)]
    public class ExposureEffectFunctionsActionCalculator : ActionCalculatorBase<IExposureEffectFunctionsActionResult> {

        public ExposureEffectFunctionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ExposureEffectFunctions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ExposureEffectFunctions][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;

            _actionDataSelectionRequirements[ScopingType.ExposureEffectFunctions].MaxSelectionCount = 1;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {

            var exposureEffectFunctions = subsetManager.AllExposureEffectFunctions;
            var list = new List<ExposureEffectFunction>();
            foreach (var eef in exposureEffectFunctions) {
                var record = new ExposureEffectFunction() {
                    Substance = eef.Substance,
                    Effect = eef.Effect,
                    TargetLevel = eef.TargetLevel,
                    ExposureRoute = eef.ExposureRoute,
                    BiologicalMatrix = eef.BiologicalMatrix,
                    DoseUnit = eef.DoseUnit,
                    ExpressionType = eef.ExpressionType,
                    EffectMetric = eef.EffectMetric,
                    Expression = eef.Expression
                };
                list.Add(record);
            }
            data.ExposureEffectFunctions = list;
        }

        protected override void summarizeActionResult(IExposureEffectFunctionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ExposureEffectFunctionsSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}

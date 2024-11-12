using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.PbkModelDefinitions {

    [ActionType(ActionType.PbkModelDefinitions)]
    public sealed class PbkModelDefinitionsActionCalculator : ActionCalculatorBase<IPbkModelDefinitionsActionResult> {

        public PbkModelDefinitionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
        }
        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new PbkModelDefinitionsSettingsSummarizer();
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            data.PbkModelDefinitions = [.. subsetManager.AllPbkModelDefinitions.Values.Where(c => !string.IsNullOrEmpty(c.FilePath))];
            foreach (var definition in data.PbkModelDefinitions) {
                definition.KineticModelDefinition = MCRAKineticModelDefinitions.GetKineticModelDefinition(definition.FilePath, definition.IdModelDefinition);
            }
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update(100);
        }
        protected override void summarizeActionResult(
            IPbkModelDefinitionsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new PbkModelDefinitionsSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}

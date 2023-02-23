using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.NonDietaryExposureSources {

    [ActionType(ActionType.NonDietaryExposureSources)]
    public class NonDietaryExposureSourcesActionCalculator : ActionCalculatorBase<INonDietaryExposureSourcesActionResult> {

        public NonDietaryExposureSourcesActionCalculator(ProjectDto project) : base(project) {
            _actionDataSelectionRequirements[ScopingType.NonDietaryExposureSources].AllowCodesInScopeNotInSource = true;
        }

        protected override void verify() {
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.NonDietaryExposureSources = subsetManager.AllNonDietaryExposureSources;
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            // Nothing
        }

        protected override void summarizeActionResult(INonDietaryExposureSourcesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new NonDietaryExposureSourcesSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}

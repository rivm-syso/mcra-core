using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Action {

    public abstract class ActionModuleResultsSummarizer<TConfig, TResult> : ActionResultsSummarizerBase<TResult>
        where TResult : IActionResult
        where TConfig : ModuleConfigBase {

        protected TConfig _configuration;

        public override ActionType ActionType => _configuration.ActionType;

        public ActionModuleResultsSummarizer(TConfig config) {
            _configuration = config;
        }
    }
}

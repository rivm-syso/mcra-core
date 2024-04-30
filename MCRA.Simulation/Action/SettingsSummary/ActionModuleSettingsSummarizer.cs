using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Action {

    public abstract class ActionModuleSettingsSummarizer<T> : ActionSettingsSummarizerBase where T : ModuleConfigBase {

        protected T _configuration;

        public override ActionType ActionType => _configuration.ActionType;

        public ActionModuleSettingsSummarizer(T config) {
            _configuration = config;
        }
    }
}

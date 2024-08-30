using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.KineticConversionFactors {

    public class KineticConversionFactorsSettingsSummarizer : ActionModuleSettingsSummarizer<KineticConversionFactorsModuleConfig> {

        public KineticConversionFactorsSettingsSummarizer(KineticConversionFactorsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            //TODO, is not relevant when no kinetic conversion factors are used
            section.SummarizeSetting(SettingsItemType.KCFSubgroupDependent, _configuration.KCFSubgroupDependent);
            return section;
        }
    }
}

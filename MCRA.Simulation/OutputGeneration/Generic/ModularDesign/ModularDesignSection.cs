using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.Linking;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModularDesignSection : SummarySection {
        public ICollection<(ActionType, ModuleType, List<string>)> AllRelations { get; set; }
        public ActionType ActionType { get; set; }
        public void Summarize(ActionMapping actionMapping) {
            var allInputModules = actionMapping.GetModuleMappings()
                .Where(c => c.IsVisible)
                .Select(c => c.ActionType.ToString())
                .ToList();
            AllRelations = ActionUtils.GetAllRelations(allInputModules);
            ActionType = actionMapping.MainActionType;
        }
    }
}

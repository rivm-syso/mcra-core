using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Action {

    public abstract class ActionResultsSummarizerBase<T>
        where T : IActionResult {

        public abstract ActionType ActionType { get; }

        public abstract void Summarize(ProjectDto project, T result, ActionData data, SectionHeader header, int order);

        protected string getSectionLabel<TEnum>(TEnum value) where TEnum : struct {
            return $"{ActionType}:{value}";
        }
    }
}

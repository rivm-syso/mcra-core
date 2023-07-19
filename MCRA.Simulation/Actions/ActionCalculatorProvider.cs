using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions {

    public sealed class ActionCalculatorProvider {

        private Dictionary<ActionType, IActionCalculator> _actionCalculators = new();

        public IActionCalculator Get(ActionType actionType, ProjectDto project, bool verify) {
            if (!_actionCalculators.ContainsKey(actionType)) {
                var actionCalculator = Create(actionType, project, verify);
                _actionCalculators[actionType] = actionCalculator;
            }
            return _actionCalculators[actionType];
        }

        public void Reset() {
            _actionCalculators = new Dictionary<ActionType, IActionCalculator>();
        }

        public static IActionCalculator Create(ActionType actionType, ProjectDto project, bool verify) {
            IActionCalculator result;
            var calculatorType = Type.GetType($"MCRA.Simulation.Actions.{actionType}.{actionType}ActionCalculator", false, true);
            result = calculatorType == null
                   ? throw new Exception($"No calculator found for action type { actionType }")
                   : (IActionCalculator)Activator.CreateInstance(calculatorType, project);

            if (verify) {
                result.Verify();
            }
            return result;
        }
    }
}

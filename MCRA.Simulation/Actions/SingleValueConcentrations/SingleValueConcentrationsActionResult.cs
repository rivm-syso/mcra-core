using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.SingleValueConcentrations {
    public class SingleValueConcentrationsActionResult : IActionResult {
        public ConcentrationUnit SingleValueConcentrationUnit { get; set; }
        public IDictionary<(Food, Compound), SingleValueConcentrationModel> MeasuredSubstanceSingleValueConcentrations { get; set; }
        public IDictionary<(Food, Compound), SingleValueConcentrationModel> ActiveSubstanceSingleValueConcentrations { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}

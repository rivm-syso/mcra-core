using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.SingleValueConsumptions {
    public sealed class SingleValueConsumptionsActionResult : IActionResult {
        public ConsumptionIntakeUnit IntakeUnit { get; set; }
        public ICollection<SingleValueConsumptionModel> SingleValueConsumptionsByModelledFood { get; set; }
        public BodyWeightUnit SingleValueConsumptionsBodyWeightUnit { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}

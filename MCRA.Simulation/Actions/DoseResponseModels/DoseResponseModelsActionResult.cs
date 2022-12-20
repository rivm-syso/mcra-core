using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.DoseResponseModels {
    public class DoseResponseModelsActionResult : IActionResult {
        public ICollection<DoseResponseModel> DoseResponseModels { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}

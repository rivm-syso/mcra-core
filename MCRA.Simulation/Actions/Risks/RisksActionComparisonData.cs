using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Actions.ActionComparison;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.Risks {
    public class RisksActionComparisonData : IActionComparisonData {
        public string IdResultSet { get; set; }
        public string NameResultSet { get; set; }
        public ICollection<RiskModel> RiskModels { get; set; }
    }
}

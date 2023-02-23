using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Actions.ActionComparison;

namespace MCRA.Simulation.Actions.TargetExposures {
    public class TargetExposuresActionComparisonData : IActionComparisonData {
        public string IdResultSet { get; set; }
        public string NameResultSet { get; set; }
        public ICollection<TargetExposureModel> TargetExposureModels { get; set; }
    }
}

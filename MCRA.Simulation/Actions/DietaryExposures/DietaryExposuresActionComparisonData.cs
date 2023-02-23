using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Actions.ActionComparison;

namespace MCRA.Simulation.Actions.DietaryExposures {
    public class DietaryExposuresActionComparisonData : IActionComparisonData {
        public string IdResultSet { get; set; }
        public string NameResultSet { get; set; }
        public ICollection<DietaryExposureModel> DietaryExposureModels { get; set; }
    }
}

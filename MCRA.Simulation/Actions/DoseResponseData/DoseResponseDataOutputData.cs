
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DoseResponseData {
    public class DoseResponseDataOutputData : IModuleOutputData {
        public ICollection<DoseResponseExperiment> AvailableDoseResponseExperiments { get; set; }
        public ICollection<DoseResponseExperiment> SelectedResponseExperiments { get; set; }
        public IModuleOutputData Copy() {
            return new DoseResponseDataOutputData() {
                AvailableDoseResponseExperiments = AvailableDoseResponseExperiments,
                SelectedResponseExperiments = SelectedResponseExperiments
            };
        }
    }
}


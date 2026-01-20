using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.OccupationalTaskExposures {
    public class OccupationalTaskExposuresOutputData : IModuleOutputData {
        public ICollection<OccupationalTaskExposure> OccupationalTaskExposures { get; set; }

        public IModuleOutputData Copy() {
            return new OccupationalTaskExposuresOutputData() {
                OccupationalTaskExposures = OccupationalTaskExposures
            };
        }
    }
}

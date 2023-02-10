
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.PointsOfDeparture {
    public class PointsOfDepartureOutputData : IModuleOutputData {
        public ICollection<PointOfDeparture> PointsOfDeparture { get; set; }
        public IModuleOutputData Copy() {
            return new PointsOfDepartureOutputData() {
                PointsOfDeparture = PointsOfDeparture
            };
        }
    }
}


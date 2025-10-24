using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation;

namespace MCRA.Simulation.Actions.OccupationalExposures {
    public class OccupationalExposuresOutputData : IModuleOutputData {

        public ICollection<OccupationalScenarioExposure> OccupationalScenarioExposures { get; set; }
        public ExposureUnitTriple OccupationalExposureUnit { get; set; }

        public IModuleOutputData Copy() {
            return new OccupationalExposuresOutputData() {
                OccupationalScenarioExposures = OccupationalScenarioExposures,
                OccupationalExposureUnit = OccupationalExposureUnit
            };
        }
    }
}


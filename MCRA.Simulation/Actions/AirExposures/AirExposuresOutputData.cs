using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Actions.AirExposures {
    public class AirExposuresOutputData : IModuleOutputData {

        public ICollection<AirIndividualExposure> IndividualAirExposures { get; set; }
        public ExposureUnitTriple AirExposureUnit { get; set; }

        public IModuleOutputData Copy() {
            return new AirExposuresOutputData() {
                IndividualAirExposures = IndividualAirExposures,
                AirExposureUnit = AirExposureUnit
            };
        }
    }
}


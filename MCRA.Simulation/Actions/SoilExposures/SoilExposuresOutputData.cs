using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SoilExposureCalculation;

namespace MCRA.Simulation.Actions.SoilExposures {
    public class SoilExposuresOutputData : IModuleOutputData {

        public ICollection<SoilIndividualExposure> IndividualSoilExposures { get; set; }
        public ExposureUnitTriple SoilExposureUnit { get; set; }

        public IModuleOutputData Copy() {
            return new SoilExposuresOutputData() {
                IndividualSoilExposures = IndividualSoilExposures,
                SoilExposureUnit = SoilExposureUnit
            };
        }
    }
}


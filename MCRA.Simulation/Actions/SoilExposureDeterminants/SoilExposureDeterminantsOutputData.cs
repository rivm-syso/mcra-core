using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SoilExposureDeterminants {
    public class SoilExposureDeterminantsOutputData : IModuleOutputData {
        public IList<SoilIngestion> SoilIngestions { get; set; }
        public ExternalExposureUnit SoilIngestionUnit { get; set; }

        public IModuleOutputData Copy() {
            return new SoilExposureDeterminantsOutputData() {
                SoilIngestions = SoilIngestions,
                SoilIngestionUnit = SoilIngestionUnit
            };
        }
    }
}


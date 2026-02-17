using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.AirExposureDeterminants {
    public class AirExposureDeterminantsOutputData : IModuleOutputData {
        public ExternalExposureUnit AirIngestionUnit { get; set; }
        public IList<AirIndoorFraction> AirIndoorFractions { get; set; }
        public IList<AirBodyExposureFraction> AirBodyExposureFractions { get; set; }
        public IList<AirVentilatoryFlowRate> AirVentilatoryFlowRates { get; set; }

        public IModuleOutputData Copy() {
            return new AirExposureDeterminantsOutputData() {
                AirIngestionUnit = AirIngestionUnit,
                AirIndoorFractions = AirIndoorFractions,
                AirVentilatoryFlowRates = AirVentilatoryFlowRates,
                AirBodyExposureFractions = AirBodyExposureFractions
            };
        }
    }
}


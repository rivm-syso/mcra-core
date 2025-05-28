using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConsumerProductExposureDeterminants {
    public class ConsumerProductExposureDeterminantsOutputData : IModuleOutputData {
        //public ExternalExposureUnit AirIngestionUnit { get; set; }
        public IList<ConsumerProductExposureFraction> ConsumerProductExposureFractions { get; set; }
        public IList<ConsumerProductApplicationAmount> ConsumerProductApplicationAmounts { get; set; }

        public IModuleOutputData Copy() {
            return new ConsumerProductExposureDeterminantsOutputData() {
                //AirIngestionUnit = AirIngestionUnit,
                ConsumerProductExposureFractions = ConsumerProductExposureFractions,
                ConsumerProductApplicationAmounts = ConsumerProductApplicationAmounts
            };
        }
    }
}


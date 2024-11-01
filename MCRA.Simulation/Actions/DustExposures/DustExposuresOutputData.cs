using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Actions.DustExposures {
    public class DustExposuresOutputData : IModuleOutputData {

        public ICollection<ExposureRoute> DustExposureRoutes { get; set; }
        public ICollection<DustIndividualDayExposure> IndividualDustExposures { get; set; }
        public ExposureUnitTriple DustExposureUnit { get; set; }

        public IModuleOutputData Copy() {
            return new DustExposuresOutputData() {
                IndividualDustExposures = IndividualDustExposures,
                DustExposureRoutes = DustExposureRoutes,
                DustExposureUnit = DustExposureUnit
            };
        }
    }
}


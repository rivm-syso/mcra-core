using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Actions.DustExposures {
    public class DustExposuresOutputData : IModuleOutputData {        
        public ICollection<ExposureRoute> DustExposureRoutes { get; set; }

        public ICollection<DustIndividualDayExposure> IndividualDustExposures { get; set; }
        public ExposureUnitTriple DustExposureUnit {
            get {
                if (IndividualDustExposures?.Any() ?? false) {
                    return IndividualDustExposures.First().ExposureUnit;
                } else {
                    return ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
                }
            }
        }
        public IModuleOutputData Copy() {
            return new DustExposuresOutputData() {
                IndividualDustExposures = IndividualDustExposures,
                DustExposureRoutes = DustExposureRoutes,
            };
        }
    }
}


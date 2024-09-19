
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Actions.DustExposures {
    public class DustExposuresOutputData : IModuleOutputData {
        public ICollection<NonDietaryExposureSet> DustExposureSets { get; set; }
        public IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> DustExposures { get; set; }
        public ICollection<ExposureRoute> DustExposureRoutes { get; set; }

        public ICollection<IndividualDustExposureRecord> IndividualDustExposures { get; set; }
        public ExternalExposureUnit DustExposureUnit {
            get {
                if (DustExposureSets?.Any() ?? false) {
                    return DustExposureSets.First().NonDietarySurvey.ExposureUnit;
                } else {
                    return ExternalExposureUnit.mgPerDay;
                }
            }
        }
        public IModuleOutputData Copy() {
            return new DustExposuresOutputData() {
                DustExposureSets = DustExposureSets,
                DustExposures = DustExposures,
                DustExposureRoutes = DustExposureRoutes,
            };
        }
    }
}


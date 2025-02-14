
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.NonDietaryExposures {
    public class NonDietaryExposuresOutputData : IModuleOutputData {
        public ICollection<NonDietaryExposureSet> NonDietaryExposureSets { get; set; }
        public IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> NonDietaryExposures { get; set; }
        public ICollection<ExposureRoute> NonDietaryExposureRoutes { get; set; }
        public ExternalExposureUnit NonDietaryExposureUnit {
            get {
                if (NonDietaryExposureSets?.Count > 0) {
                    return NonDietaryExposureSets.First().NonDietarySurvey.ExposureUnit;
                } else {
                    return ExternalExposureUnit.mgPerDay;
                }
            }
        }
        public IModuleOutputData Copy() {
            return new NonDietaryExposuresOutputData() {
                NonDietaryExposureSets = NonDietaryExposureSets,
                NonDietaryExposures = NonDietaryExposures,
                NonDietaryExposureRoutes = NonDietaryExposureRoutes,
            };
        }
    }
}


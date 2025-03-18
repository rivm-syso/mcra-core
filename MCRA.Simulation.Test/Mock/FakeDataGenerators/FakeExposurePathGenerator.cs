using MCRA.General;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock exposure paths
    /// </summary>
    public static class FakeExposurePathGenerator {

        /// <summary>
        /// Creates a list of target individual day exposures
        /// </summary>
        public static List<ExposurePath> Create(
            params ExposureRoute[] routes
        ) {
            return [.. routes.Select(r => 
                new ExposurePath(DefaultForRoute(r), r)
            )];
        }

        /// <summary>
        /// For testing only: an appropriate default for each possible route.
        /// </summary>
        public static ExposureSource DefaultForRoute(ExposureRoute route) {
            return route switch {
                ExposureRoute.Oral => ExposureSource.Diet,
                ExposureRoute.Dermal => ExposureSource.Dust,
                ExposureRoute.Inhalation => ExposureSource.Dust,
                _ => ExposureSource.Undefined,
            };
        }
    }
}

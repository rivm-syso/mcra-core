namespace MCRA.General {
    public static class ExposureRouteExtensions {

        public static ExposurePathType GetExposurePath(
            this ExposureRoute exposureRoute
        ) {
            return exposureRoute switch {
                ExposureRoute.Oral => ExposurePathType.Oral,
                ExposureRoute.Dermal => ExposurePathType.Dermal,
                ExposureRoute.Inhalation => ExposurePathType.Inhalation,
                _ => ExposurePathType.Undefined,
            };
        }
    }
}

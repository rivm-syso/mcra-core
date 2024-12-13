namespace MCRA.General {
    public static class ExposurePathTypeExtensions {

        public static ExposureRoute GetExposureRoute(this ExposurePathType exposurePathType) {
            return exposurePathType switch {
                ExposurePathType.Dietary or ExposurePathType.Oral => ExposureRoute.Oral,
                ExposurePathType.Dermal => ExposureRoute.Dermal,
                ExposurePathType.Inhalation => ExposureRoute.Inhalation,
                _ => ExposureRoute.Undefined,
            };
        }
    }
}

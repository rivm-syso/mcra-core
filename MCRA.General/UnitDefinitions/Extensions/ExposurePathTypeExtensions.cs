namespace MCRA.General {
    public static class ExposurePathTypeExtensions {

        public static ExposureRoute GetExposureRoute(this ExposurePathType exposurePathType) {
            switch (exposurePathType) {
                case ExposurePathType.Dietary:
                case ExposurePathType.Oral:
                    return ExposureRoute.Oral;
                case ExposurePathType.Dermal:
                    return ExposureRoute.Dermal;
                case ExposurePathType.Inhalation:
                    return ExposureRoute.Inhalation;
                default:
                    return ExposureRoute.Undefined;
            }
        }
    }
}

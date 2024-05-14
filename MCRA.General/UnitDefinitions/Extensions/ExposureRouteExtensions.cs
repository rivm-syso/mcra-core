namespace MCRA.General {
    public static class ExposureRouteExtensions {

        public static ExposurePathType GetExposurePath(
            this ExposureRoute exposureRoute
        ) {
            switch (exposureRoute) {
                case ExposureRoute.Oral:
                    return ExposurePathType.Oral;
                case ExposureRoute.Dermal:
                    return ExposurePathType.Dermal;
                case ExposureRoute.Inhalation:
                    return ExposurePathType.Inhalation;
                case ExposureRoute.Undefined:
                default:
                    return ExposurePathType.Undefined;
            }
        }
    }
}

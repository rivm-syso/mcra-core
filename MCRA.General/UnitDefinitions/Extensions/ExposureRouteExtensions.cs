namespace MCRA.General {
    public static class ExposureRouteExtensions {

        public static ExposurePathType GetExposurePath(
            this ExposureRoute exposureRoute,
            bool isDietary = true
        ) {
            switch (exposureRoute) {
                case ExposureRoute.Oral:
                    return isDietary ? ExposurePathType.Dietary : ExposurePathType.Oral;
                case ExposureRoute.Dermal:
                    return ExposurePathType.Undefined;
                case ExposureRoute.Inhalation:
                    return ExposurePathType.Undefined;
                case ExposureRoute.Undefined:
                default:
                    return ExposurePathType.Undefined;
            }
        }
    }
}

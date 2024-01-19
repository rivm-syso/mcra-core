namespace MCRA.General {
    public partial class ExposureRouteConverter : UnitConverterBase<ExposureRoute> {
        public static ExposureRoute FromString(string str) {
            return FromString(str, ExposureRoute.Undefined);
        }
    }
}

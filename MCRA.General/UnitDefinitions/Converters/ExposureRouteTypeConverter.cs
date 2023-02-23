namespace MCRA.General {
    public partial class ExposureRouteTypeConverter : UnitConverterBase<ExposureRouteType> {
        /// <summary>
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ExposureRouteType FromString(string str) {
            return FromString(str, ExposureRouteType.Undefined);
        }
    }
}

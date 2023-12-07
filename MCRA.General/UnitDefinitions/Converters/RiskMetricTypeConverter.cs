namespace MCRA.General {
    public partial class RiskMetricTypeConverter : UnitConverterBase<RiskMetricType> {
        /// <summary>
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static RiskMetricType FromString(string str) {
            return FromString(str, RiskMetricType.HazardExposureRatio);
        }
    }
}
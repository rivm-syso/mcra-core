namespace MCRA.General {
    public class HarvestApplicationTypeConverter : UnitConverterBase<HarvestApplicationType> {
        /// <summary>
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static HarvestApplicationType FromString(string str) => FromString(str, HarvestApplicationType.Undefined);
    }
}

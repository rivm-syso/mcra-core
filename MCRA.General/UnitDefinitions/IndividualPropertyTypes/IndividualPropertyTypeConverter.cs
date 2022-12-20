namespace MCRA.General {

    public class IndividualPropertyTypeConverter : UnitConverterBase<IndividualPropertyType> {
        /// <summary>
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IndividualPropertyType FromString(string str) => FromString(str, IndividualPropertyType.Numeric);
    }
}

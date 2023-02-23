namespace MCRA.General {

    public partial class IndividualPropertyTypeConverter {
        /// <summary>
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IndividualPropertyType FromString(string str) => FromString(str, IndividualPropertyType.Numeric);
    }
}

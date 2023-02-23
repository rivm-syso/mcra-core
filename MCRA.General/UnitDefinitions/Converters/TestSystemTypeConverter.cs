namespace MCRA.General {
    public partial class TestSystemTypeConverter {
        /// <summary>
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static TestSystemType FromString(string str) => FromString(str, TestSystemType.Undefined);
    }
}

namespace MCRA.General {
    public partial class MonthTypeConverter {
        /// <summary>
        /// Check the string as an month unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static void CheckString(string str) {
            var months = str.Split(',');
            foreach (var month in months) {
                UnitDefinition.FromString<MonthType>(month);
            }
        }
    }
}

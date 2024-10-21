namespace MCRA.General {
    public partial class DoseResponseModelTypeConverter : UnitConverterBase<DoseResponseModelType> {
        /// <summary>
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DoseResponseModelType TryGetFromString(string str) {
            return UnitDefinition.FromString(str, allowInvalidString: true, DoseResponseModelType.Unknown);
        }
    }
}

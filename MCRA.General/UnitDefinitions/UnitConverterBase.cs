
namespace MCRA.General {
    public class UnitConverterBase<T> {
        private static UnitDefinition _unitDefinition;

        /// <summary>
        /// Gets the unit definitions of this unit.
        /// </summary>
        public static UnitDefinition UnitDefinition {
            get {
                if (_unitDefinition == null) {
                    _unitDefinition = McraUnitDefinitions.Instance.UnitDefinitions[typeof(T).Name];
                }
                return _unitDefinition;
            }
        }

        /// <summary>
        /// Parses the string as unit type.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        public static T FromString(string str, T defaultType = default, bool allowInvalidString = false) {
            if (!string.IsNullOrEmpty(str) &&
                (UnitDefinition.UndefinedAliases == null ||
                 !UnitDefinition.UndefinedAliases.Contains(str, StringComparer.OrdinalIgnoreCase))
            ) {
                return UnitDefinition.FromString(str, allowInvalidString, defaultType);
            }
            return defaultType;
        }

        /// <summary>
        /// Parses the string as unit type.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        public static T FromUri(string str, T defaultType = default, bool allowInvalidString = false) {
            if (!string.IsNullOrEmpty(str)
            ) {
                return UnitDefinition.FromUri(str, allowInvalidString, defaultType);
            }
            return defaultType;
        }
    }
}

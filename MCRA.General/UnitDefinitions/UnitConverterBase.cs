
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
        public static T FromString(string str, T defaultType = default) {
            if (!string.IsNullOrEmpty(str)) {
                return UnitDefinition.FromString<T>(str);
            }
            return defaultType;
        }

        /// <summary>
        /// Parses the string as unit type.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        public static T TryGetFromString(string str, T defaultType = default) {
            if (!string.IsNullOrEmpty(str)) {
                return UnitDefinition.TryGetFromString(str, defaultType);
            }
            return defaultType;
        }
    }
}

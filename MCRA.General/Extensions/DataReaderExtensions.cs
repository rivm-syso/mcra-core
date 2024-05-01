using System.Data;

namespace MCRA.General.Extensions {
    public static class DataReaderExtensions {
        private static int map(int[] fieldMap, Enum fieldEnum) {
            var enumIdx = Convert.ToInt32(fieldEnum);
            return fieldMap?[enumIdx] ?? enumIdx;
        }

        public static float GetFloat(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return (float)r.GetDouble(index);
        }

        public static float? GetFloatOrNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return (index == -1 || r.IsDBNull(index)) ? null : (float)r.GetDouble(index);
        }

        public static double GetDouble(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return r.GetDouble(index);
        }

        public static double? GetDoubleOrNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return (index == -1 || r.IsDBNull(index)) ? null : r.GetDouble(index);
        }

        public static bool GetBoolean(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            var value = r.GetValue(index);
            if (!bool.TryParse(value.ToString(), out bool result)) {
                result = Convert.ToInt32(value) != 0;
            }
            return result;
        }

        public static bool? GetBooleanOrNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            if (index == -1 || r.IsDBNull(index)) {
                return null;
            }

            var value = r.GetValue(index);
            if (!bool.TryParse(value.ToString(), out bool result)) {
                result = Convert.ToInt32(value) != 0;
            }
            return result;
        }

        public static int GetInt32(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return r.GetInt32(index);
        }

        public static int? GetIntOrNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return (index == -1 || r.IsDBNull(index)) ? null : r.GetInt32(index);
        }

        public static T GetEnum<T>(this IDataReader r, Enum fieldEnum, int[] fieldMap, T defaultValue = default) where T : Enum {
            return UnitConverterBase<T>.TryGetFromString(r.GetStringOrNull(fieldEnum, fieldMap), defaultValue);
        }

        public static string GetString(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return r.GetString(index).Trim();
        }

        public static string GetStringOrNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return (index == -1 || r.IsDBNull(index)) ? null : r.GetString(index).Trim();
        }

        public static DateTime GetDateTime(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return r.GetDateTime(index);
        }

        public static DateTime? GetDateTimeOrNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return (index == -1 || r.IsDBNull(index)) ? null : r.GetDateTime(index);
        }

        public static object GetValue(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return r.GetValue(index);
        }

        public static object GetValueOrNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return (index == -1 || r.IsDBNull(index)) ? null : r.GetValue(index);
        }

        public static bool IsDBNull(this IDataReader r, Enum fieldEnum, int[] fieldMap) {
            var index = map(fieldMap, fieldEnum);
            return index == -1 || r.IsDBNull(index);
        }
    }
}

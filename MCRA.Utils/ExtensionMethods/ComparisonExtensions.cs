using System.Reflection;

namespace MCRA.Utils.ExtensionMethods {
    public static class ComparisonExtensions {

        /// <summary>
        /// Compares the public properties of the object with another object of the same type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="ignore">The names of the properties to ignore.</param>
        /// <returns></returns>
        public static bool ComparePublicProperties<T>(this T self, T other, params string[] ignore) where T : class {
            if (self != null && other != null) {
                var type = typeof(T);
                var ignoreList = new List<string>(ignore);
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                    if (!ignoreList.Contains(property.Name)) {
                        var selfValue = type.GetProperty(property.Name).GetValue(self, null);
                        var otherValue = type.GetProperty(property.Name).GetValue(other, null);
                        if (selfValue != otherValue && (selfValue == null || !selfValue.Equals(otherValue))) {
                            return false;
                        }
                    }
                }
                return true;
            }
            return self == other;
        }
    }
}

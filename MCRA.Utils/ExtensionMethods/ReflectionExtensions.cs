using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace MCRA.Utils.ExtensionMethods {
    public static class ReflectionExtensions {

        /// <summary>
        /// Returns the member's attribute of type T. Throws an error when this attribute
        /// is required, but not found for the member.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo member, bool isRequired)
            where T : Attribute {
            var attribute = member.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            if (attribute == null && isRequired) {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The {0} attribute must be defined on member {1}", typeof(T).Name, member.Name));
            }
            return (T)attribute;
        }

        /// <summary>
        /// Returns the attributes of type T of the member. Throws an error when this attribute
        /// is required, but not found for the member.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        public static T[] GetAttributes<T>(this MemberInfo member, bool isRequired)
            where T : Attribute {
            var attributes = (T[])member.GetCustomAttributes(typeof(T), true);
            return attributes;
        }

        /// <summary>
        /// Gets "Name" or "ShortName" property of the DisplayAttribute. If both are unspecified gets the DisplayNameAttribute value. If no
        /// 'name' attributes are specified, the propertyname itself it returned.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetDisplayName(this MemberInfo memberInfo) {
            // Get display attribute
            var displayAttr = memberInfo.GetAttribute<DisplayAttribute>(false);

            // Get the name attribute
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.GetName())) {
                return displayAttr.GetName();
            }

            // Get the ShortName attribute
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.GetShortName())) {
                return displayAttr.GetShortName();
            }

            // Get DisplayName attribute
            var displayNameAttr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (displayNameAttr != null) {
                return displayNameAttr.DisplayName;
            }

            return memberInfo.Name;
        }

        /// <summary>
        /// Gets the Description property of the DisplayAttribute or the Description property of the DescriptionAttribute.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetDescription(this MemberInfo memberInfo) {
            // Get display attribute
            var displayAttr = memberInfo.GetAttribute<DisplayAttribute>(false);
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.GetDescription())) {
                return displayAttr.GetDescription();
            }

            // Get DisplayName attribute
            var descriptionAttribute = memberInfo.GetAttribute<DescriptionAttribute>(false);
            if (descriptionAttribute != null) {
                return descriptionAttribute.Description;
            }
            return null;
        }

        /// <summary>
        /// Gets "ShortName" property of the DisplayAttribute. If unspecified, the propertyname itself is returned.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetShortName(this MemberInfo memberInfo) {
            var displayAttr = memberInfo.GetAttribute<DisplayAttribute>(false);
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.GetName())) {
                return displayAttr.GetName();
            }
            if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.GetShortName())) {
                return displayAttr.GetShortName();
            }
            var displayNameAttr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (displayNameAttr != null) {
                return displayNameAttr.DisplayName;
            }
            return memberInfo.Name;
        }

        /// <summary>
        /// Gets "Order" property of the DisplayAttribute.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static int? GetOrder(this MemberInfo memberInfo) {
            var displayAttr = memberInfo.GetAttribute<DisplayAttribute>(false);
            return displayAttr.GetOrder();
        }

        /// <summary>
        /// Returns the display name of the class
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetClassDisplayName(this MemberInfo memberInfo) {
            var attribute = memberInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                .Cast<DisplayNameAttribute>()
                .SingleOrDefault();
            if (attribute == null) {
                return memberInfo.Name;
            }
            return attribute.DisplayName;
        }

        /// <summary>
        /// Returns the description of the class.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetClassDescription(this MemberInfo memberInfo) {
            var attribute = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .SingleOrDefault();
            if (attribute == null) {
                return string.Empty;
            }
            return attribute.Description;
        }

        /// <summary>
        /// Returns the visible properties of the record type. The hidden properties
        /// list can be included to specify additional hidden properties.
        /// </summary>
        /// <param name="recordType"></param>
        /// <param name="hiddenProperties"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetVisibleProperties(this Type recordType, IList<string> hiddenProperties) {
            var visibleProperties = new List<PropertyInfo>();
            foreach (var property in recordType.GetProperties().Where(prop => prop.CanRead)) {
                if (IsPropertyVisible(property, hiddenProperties)) {
                    visibleProperties.Add(property);
                }
            }
            return visibleProperties.OrderBy(p => {
                var displayAttribute = p.GetAttribute<DisplayAttribute>(false);
                return displayAttribute?.GetOrder();
            });
        }

        /// <summary>
        /// Returns whether the property is visible. I.e., if the property name is not included in
        /// the hidden properties list and whether it does not have the AutoGenerateField attribute
        /// being false.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="hiddenProperties"></param>
        /// <returns></returns>
        public static bool IsPropertyVisible(PropertyInfo property, IList<string> hiddenProperties) {
            if (hiddenProperties != null && hiddenProperties.Contains(property.Name)) {
                return false;
            }
            var displayAttribute = property.GetAttribute<DisplayAttribute>(false);
            if (displayAttribute != null && displayAttribute.GetAutoGenerateField() == false) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the propertyInfo for the property that is selected with the lambda expression.
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TProperty">The target property type</typeparam>
        /// <param name="source"></param>
        /// <param name="propertySelector">lambda expression to select a property</param>
        /// <returns></returns>
        /// <see cref="http://stackoverflow.com/questions/671968/retrieving-property-name-from-lambda-expression"/>
        public static PropertyInfo GetPropertyInfoFor<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertySelector)
            where TSource : class {
            Type sourceType = typeof(TSource);

            MemberExpression member = propertySelector.Body as MemberExpression;
            if (member == null) {
                throw new ArgumentException($"Expression '{propertySelector}' refers to a method, not a property.");
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null) {
                throw new ArgumentException($"Expression '{propertySelector}' refers to a field, not a property.");
            }

            if (sourceType != propInfo.ReflectedType && !sourceType.IsSubclassOf(propInfo.ReflectedType)) {
                throw new ArgumentException($"Expression '{propertySelector}' refers to a property that is not from type {sourceType}.");
            }

            return propInfo;
        }

        /// <summary>
        /// Check's whether the target property has one or more attributes of the target type.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static bool HasAttribute(this PropertyInfo p, Type attributeType) {
            return p.GetCustomAttributes(attributeType, false).Any();
        }

        /// <summary>
        /// Returns the property with the specified name, or null if the property does
        /// not exist.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertySingleOrDefault(this Type type, string name) {
            return type.GetProperties().SingleOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Returns a string with the value of the property formatted according to its type
        /// and its formatting attributes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="metaTypeProperty"></param>
        /// <returns></returns>
        public static string PrintPropertyValue<T>(this T source, PropertyInfo property, PropertyInfo metaTypeProperty = null) {
            var formatAttribute =
                metaTypeProperty?.GetAttribute<DisplayFormatAttribute>(false) ??
                property?.GetAttribute<DisplayFormatAttribute>(false);

            var isInteger = property.PropertyType == typeof(int)
                || property.PropertyType == typeof(int?);
            var isFloat = property.PropertyType == typeof(float)
                || property.PropertyType == typeof(float?);
            var isDouble = property.PropertyType == typeof(double)
                || property.PropertyType == typeof(double?);
            var isDecimal = property.PropertyType == typeof(decimal)
                || property.PropertyType == typeof(decimal?);
            var isBool = property.PropertyType == typeof(bool)
                || property.PropertyType == typeof(bool?);

            var isEnum = property.PropertyType.IsSubclassOf(typeof(Enum));
            var propertyValue = property.GetValue(source, null);

            if (propertyValue == null
                || (isDouble && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isFloat && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isDecimal && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isInteger && Convert.ToInt32(propertyValue).Equals(int.MaxValue))
            ) {
                return "-";
            } else if (formatAttribute != null) {
                var value = string.Format(CultureInfo.InvariantCulture, formatAttribute.DataFormatString, propertyValue);
                return value.Replace(" %", string.Empty);
            } else if (isEnum) {
                return ((Enum)propertyValue).GetDisplayName();
            } else if (isBool) {
                return ((bool)propertyValue) ? "yes" : "no";
            } else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<double>))) {
                return string.Join(", ", (propertyValue as IEnumerable<double>).Select(s => s.ToString(CultureInfo.InvariantCulture)));
            } else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<int>))) {
                return string.Join(", ", propertyValue as IEnumerable<int>);
            } else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<string>))) {
                return string.Join(", ", propertyValue as IEnumerable<string>);
            } else if (isDouble || isDecimal || isFloat) {
                return Convert.ToDouble(propertyValue).ToString(CultureInfo.InvariantCulture);
            } else {
                return propertyValue.ToString();
            }
        }

        public static string PrintValue<T>(this T source, DisplayFormatAttribute formatAttribute = null) {
            var isInteger = typeof(T) == typeof(int)
                || typeof(T) == typeof(int?);
            var isFloat = typeof(T) == typeof(float)
                || typeof(T) == typeof(float?);
            var isDouble = typeof(T) == typeof(double)
                || typeof(T) == typeof(double?);
            var isDecimal = typeof(T) == typeof(decimal)
                || typeof(T) == typeof(decimal?);

            var isEnum = typeof(T).IsSubclassOf(typeof(Enum));
            var propertyValue = source;

            if (propertyValue == null
                || (isDouble && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isFloat && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isDecimal && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isInteger && Convert.ToInt32(propertyValue).Equals(int.MaxValue))
            ) {
                return "-";
            } else if (formatAttribute != null) {
                var value = string.Format(CultureInfo.InvariantCulture, formatAttribute.DataFormatString, source);
                return value.Replace(" %", string.Empty);
            } else if (isEnum) {
                return (source as Enum).GetDisplayName();
            } else if (typeof(T) == typeof(bool)) {
                return Convert.ToBoolean(source) ? "Yes" : "No";
            } else if (typeof(T).IsAssignableTo(typeof(IEnumerable<double>))) {
                return string.Join(", ", (source as IEnumerable<double>).Select(s => s.ToString(CultureInfo.InvariantCulture)));
            } else if (typeof(T).IsAssignableTo(typeof(IEnumerable<int>))) {
                return string.Join(", ", source as IEnumerable<int>);
            } else if (typeof(T).IsAssignableTo(typeof(IEnumerable<string>))) {
                return string.Join(", ", source as IEnumerable<string>);
            } else if (isDouble || isDecimal || isFloat) {
                return Convert.ToDouble(source).ToString(CultureInfo.InvariantCulture);
            } else {
                return source.ToString();
            }
        }

        public static string PrintValue(this Type type, object source, string dataFormatString = null) {
            var isInteger = type == typeof(int)
                || type == typeof(int?);
            var isFloat = type == typeof(float)
                || type == typeof(float?);
            var isDouble = type == typeof(double)
                || type == typeof(double?);
            var isDecimal = type == typeof(decimal)
                || type == typeof(decimal?);

            var isEnum = type.IsSubclassOf(typeof(Enum));
            var propertyValue = source;

            if (propertyValue == null
                || (isDouble && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isFloat && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isDecimal && double.IsNaN(Convert.ToDouble(propertyValue)))
                || (isInteger && Convert.ToInt32(propertyValue).Equals(int.MaxValue))
            ) {
                return "-";
            } else if (!string.IsNullOrWhiteSpace(dataFormatString)) {
                var value = string.Format(CultureInfo.InvariantCulture, dataFormatString, source);
                return value.Replace(" %", string.Empty);
            } else if (isEnum) {
                return (source as Enum).GetDisplayName();
            } else if (type == typeof(bool)) {
                return Convert.ToBoolean(source) ? "Yes" : "No";
            } else if (type.IsAssignableTo(typeof(IEnumerable<double>))) {
                return string.Join(", ", (source as IEnumerable<double>).Select(s => s.ToString(CultureInfo.InvariantCulture)));
            } else if (type.IsAssignableTo(typeof(IEnumerable<int>))) {
                return string.Join(", ", source as IEnumerable<int>);
            } else if (type.IsAssignableTo(typeof(IEnumerable<string>))) {
                return string.Join(", ", source as IEnumerable<string>);
            } else if (isDouble || isDecimal || isFloat) {
                return Convert.ToDouble(source).ToString(CultureInfo.InvariantCulture);
            } else {
                return source.ToString();
            }
        }
    }
}

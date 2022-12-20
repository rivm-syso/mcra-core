using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Utils.ExtensionMethods {

    public static class EnumExtensions {

        /// <summary>
        /// Returns the value of the target enum's display or description attribute. If not specified,
        /// the ToString() method's response is returned.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="toLower">Specifies whether the display name should be returned in lower case.</param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum value, bool toLower = false) {
            var displayAttribute = value.GetDisplayAttribute();
            string name;
            if (displayAttribute != null) {
                name = displayAttribute.GetName();
            } else {
                var descriptionAttribute = value.GetDescriptionAttribute();
                if (descriptionAttribute != null) {
                    name = descriptionAttribute.Description;
                } else {
                    name = value.ToString();
                }
            }
            if (toLower) {
                return name.ToLowerInvariant();
            } else {
                return name;
            }
        }

        /// <summary>
        /// Returns the short display name of this enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetShortDisplayName(this Enum value) {
            return value.GetDisplayAttribute()?.ShortName ?? value.GetDisplayName();
        }

        /// <summary>
        /// Returns the short display name of this enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value) {
            var descriptionAttribute = value.GetDescriptionAttribute();
            if (descriptionAttribute != null) {
                return descriptionAttribute.Description;
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the value of the target enum's display attribute. If not specified,
        /// the ToString() method's response is returned.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DisplayAttribute GetDisplayAttribute(this Enum value) {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = (DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length > 0) {
                return attributes[0];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Returns the value of the target enum's display attribute. If not specified,
        /// the ToString() method's response is returned.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DescriptionAttribute GetDescriptionAttribute(this Enum value) {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo != null) {
                var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0) {
                    return attributes[0];
                }
            }
            return null;
        }
    }
}

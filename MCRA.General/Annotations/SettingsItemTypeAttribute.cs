using System;
using System.Reflection;
using MCRA.Utils.ExtensionMethods;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Annotations {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SettingsItemTypeAttribute : Attribute {

        public SettingsItemType SettingsItemType { get; private set; }

        public SettingsItemTypeAttribute(SettingsItemType settingsItemType) {
            this.SettingsItemType = settingsItemType;
        }

        public static SettingsItem GetSettingsItem(MemberInfo t) {
            var attribute = t.GetAttribute<SettingsItemTypeAttribute>(false);
            if (attribute != null) {
                return McraSettingsDefinitions.Instance.SettingsDefinitions[attribute.SettingsItemType];
            }
            return null;
        }
    }
}

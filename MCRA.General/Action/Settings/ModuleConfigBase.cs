using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Xml;
using MCRA.General.SettingsDefinitions;
using MCRA.Utils.Xml;

namespace MCRA.General.Action.Settings {
    public abstract class ModuleConfigBase {
        public virtual ActionType ActionType { get; } = ActionType.Unknown;
        public abstract ModuleConfiguration AsConfiguration();
        public abstract void Apply(ModuleConfiguration config);
        public abstract void Apply(SettingsItemType settingType, string rawValue);
        public abstract object GetValue(SettingsItemType settingType);

        //name for XML elements in scalar value lists
        private const string ValueElementName = "Value";
        protected ProjectDto _project = null;

        public string Id => _project?.Id.ToString();
        public string Name => _project?.Name;
        public string Description => _project?.Description;

        public static T ConvertValue<T>(ModuleSetting setting) {
            if (setting == null) {
                return default;
            }
            var typeInfo = typeof(T).GetTypeInfo();

            if (setting.XmlValues?.Any() ?? false) {
                var item = setting.XmlValues[0];
                return XmlSerialization.FromXml<T>(item.OuterXml, typeof(T));
            }

            if (typeInfo.IsEnum) {
                //try parsing the value, set to default if the enum value can't be parsed
                if (!Enum.TryParse(typeof(T), setting.Value, true, out var enumValue)) {
                    //retrieve the default from the settings definitions
                    var defaultStringValue = McraSettingsDefinitions.Instance.SettingsDefinitions[setting.Id].DefaultValue;
                    var defaultEnumValue = Enum.Parse(typeof(T), defaultStringValue);
                    enumValue = UnitConverterBase<T>.FromString(setting.Value, (T)defaultEnumValue, allowInvalidString: true);
                }
                return (T)enumValue;
            }
            return (T)Convert.ChangeType(setting.Value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static List<T> ConvertList<T>(ModuleSetting setting) {
            if (setting == null || setting.XmlValues == null || !setting.XmlValues.Any()) {
                return [];
            }

            //scalar value lists are lists with elements where name of element is 'v' or 'V'
            if (!setting.XmlValues[0].Name.Equals(ValueElementName, StringComparison.OrdinalIgnoreCase)) {
                var list = setting.XmlValues
                    .Select(v => XmlSerialization.FromXml<T>(v.OuterXml, typeof(T)))
                    .ToList();
                return list;
            }

            List<T> values;
            if (typeof(T) == typeof(string)) {
                //strings can be returned directly
                values = setting.XmlValues
                    .Select(v => v.InnerText)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Cast<T>()
                    .ToList() ?? [];
            } else if (typeof(T).IsEnum) {
                values = setting.XmlValues
                    .Select(v => v.InnerText)
                    .Select(s => Enum.Parse(typeof(T), s))
                    .Cast<T>()
                    .ToList() ?? [];
            } else {
                //cast to requested type
                values = setting.XmlValues
                    .Select(v => v.InnerText)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => Convert.ChangeType(s, typeof(T), CultureInfo.InvariantCulture))
                    .Cast<T>()
                    .ToList() ?? [];
            }

            return values;
        }

        public static void SetSetting<T>(ModuleConfiguration config, SettingsItemType key, T value) {
            if (config != null) {
                config.SettingsDictionary[key] = ToSetting(key, value);
            }
        }

        public static T GetSetting<T>(ModuleConfiguration config, SettingsItemType key, T defaultValue) {
            if (config != null && config.SettingsDictionary.TryGetValue(key, out var setting)) {
                return ConvertValue<T>(setting);
            }
            return defaultValue;
        }

        public static T GetSetting<T>(SettingsItemType settingType, string value) {
            var moduleSetting = new ModuleSetting {
                Id = settingType,
                Value = value
            };
            return ConvertValue<T>(moduleSetting);
        }

        public static List<T> GetListSetting<T>(SettingsItemType settingType, string value) {
            var moduleSetting = new ModuleSetting {
                Id = settingType,
                Value = value
            };
            return ConvertList<T>(moduleSetting);
        }

        public static List<T> GetListSetting<T>(ModuleConfiguration config, SettingsItemType key, List<T> defaultValue) {
            if (config != null && config.SettingsDictionary.TryGetValue(key, out var setting)) {
                return ConvertList<T>(setting);
            }
            return defaultValue;
        }

        public static ModuleSetting ToSetting<T>(SettingsItemType key, T value) {
            var setting = new ModuleSetting { Id = key };
            if (value is double dv) {
                setting.Value = dv.ToString(CultureInfo.InvariantCulture);
                return setting;
            }
            if (value is bool bv) {
                setting.Value = bv ? "true" : "false";
                return setting;
            }
            if (value is string s) {
                setting.Value = s;
                return setting;
            }
            var typeInfo = typeof(T).GetTypeInfo();
            if (typeInfo.IsEnum || typeInfo.IsValueType) {
                setting.Value = value.ToString();
                return setting;
            }
            if (value == null) {
                return setting;
            }
            var isList = typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(List<>);
            if (isList) {
                typeInfo = typeInfo.GetGenericArguments()[0].GetTypeInfo();
            }
            var isString = typeInfo.UnderlyingSystemType == typeof(string);

            var xmlDoc = new XmlDocument();

            if (typeInfo.IsClass && !isString && !typeInfo.IsEnum) {
                var xml = XmlSerialization.ToXml(value);
                xmlDoc.LoadXml(xml);
                setting.XmlValues = isList
                    ? xmlDoc.DocumentElement.ChildNodes.Cast<XmlElement>().ToArray()
                    : new[] { xmlDoc.DocumentElement };
            } else {
                string[] values;
                if (typeInfo.UnderlyingSystemType == typeof(double) || typeInfo.UnderlyingSystemType == typeof(float)) {
                    values = ((IEnumerable)value)
                        .Cast<double>()
                        .Select(d => d.ToString(CultureInfo.InvariantCulture))
                        .ToArray();
                } else if (typeInfo.UnderlyingSystemType == typeof(bool)) {
                    values = ((IEnumerable)value)
                        .Cast<bool>()
                        .Select(d => d ? "true" : "false")
                        .ToArray();
                } else {
                    var separator = isString ? '\n' : ' ';
                    values = ((IEnumerable)value)
                        .Cast<object>()
                        .Select(v => v.ToString())
                        .ToArray();
                }
                //create xml elements
                setting.XmlValues = values.Select(s => {
                    var element = xmlDoc.CreateElement(ValueElementName);
                    element.InnerText = s;
                    return element;
                }).ToArray();
            }
            return setting;
        }

        public static ModuleConfigBase Create(ProjectDto project, ModuleConfiguration config) => createInstance(config.ActionType, project, config);

        public static ModuleConfigBase Create(ProjectDto project, ActionType actionType) => createInstance(actionType, project);

        private static ModuleConfigBase createInstance(ActionType actionType, ProjectDto project = null, ModuleConfiguration config = null) {
            ModuleConfigBase result;
            var asm = typeof(ModuleConfigBase).Assembly;
            var configType = asm.GetType($"MCRA.General.ModuleDefinitions.Settings.{actionType}ModuleConfig", false, true);
            if (configType == null) {
                return null;
            }
            if (config == null) {
                result = (ModuleConfigBase)Activator.CreateInstance(configType);
            } else {
                result = (ModuleConfigBase)Activator.CreateInstance(configType, config);
            }
            result._project = project;
            return result;
        }

        public override string ToString() {
            return $"{ActionType}ModuleConfig";
        }
    }
}

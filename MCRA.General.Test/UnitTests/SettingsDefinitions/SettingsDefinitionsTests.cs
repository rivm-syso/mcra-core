using MCRA.General.SettingsDefinitions;
using MCRA.Utils.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.General.Test.UnitTests.SettingsDefinitions {
    [TestClass]
    public class SettingsDefinitionsTests {
        [TestMethod]
        public void SettingsDefinitionsTests_TestCompleteness() {
            var definitionsInstance = McraSettingsDefinitions.Instance;
            var definitions = definitionsInstance.SettingsDefinitions.OrderBy(c => c.Key.ToString()).ToList();

            var enumValues = Enum.GetValues(typeof(SettingsItemType))
                .Cast<SettingsItemType>()
                .Where(r => r != SettingsItemType.Undefined);

            // Check whether there is a definition for each enum value.
            var booleanTypes = new List<string>();
            var enumTypes = new List<string>();
            var numericTypes = new List<string>();
            var alphaNumericTypes = new List<string>();
            var emptyTypes = new List<string>();

            foreach (var value in enumValues) {
                Assert.IsTrue(definitionsInstance.SettingsDefinitions.ContainsKey(value), $"Missing setting definition for {value}.");
                var definition = definitionsInstance.SettingsDefinitions[value];
                if (definition.ValueType == "Boolean") {
                    booleanTypes.Add($"{definition.Id}_ {definition.Name}");
                } else if (definition.ValueType == "AlphaNumeric") {
                    alphaNumericTypes.Add($"{definition.Id}_ {definition.Name}");
                } else if (definition.ValueType == "Numeric") {
                    numericTypes.Add($"{definition.Id}_ {definition.Name}");
                } else if (definition.ValueType != null) {
                    enumTypes.Add($"{definition.Id}_ {definition.Name}");
                } else {
                    emptyTypes.Add($"{definition.Id}_ {definition.Name}");
                }
                Assert.IsNotNull(definition);
                Assert.IsNotNull(definition.ValueType);
            }
        }
        /// <summary>
        /// Checkes whether all enum values are in XML and checks equality of  Names and Description of enums compared to XML. 
        /// </summary>
        [TestMethod]
        public void SettingTypeEnumsDefinitionsTests_TestCompleteness() {
            var definitionsInstance = McraSettingTypeEnumDefinitions.Instance;

            var enums = Enum.GetValues(typeof(SettingsItemType))
               .Cast<SettingsItemType>()
               .Where(r => r != SettingsItemType.Undefined);

            var assembly = typeof(SettingsItemType).Assembly;
            foreach (var value in enums) {
                if (definitionsInstance.SettingTypeEnumDefinitions.TryGetValue(value.ToString(), out var definition)) {
                    var classType = assembly.GetType($"MCRA.General.{value}");
                    var enumValues = Enum.GetValues(classType);
                    foreach (var enumValue in enumValues) {
                        var match = definition.Units.Where(c => c.Id == enumValue.ToString()).FirstOrDefault();
                        if (match != null) {
                            var memberInfo = classType.GetMember(enumValue.ToString()).First();
                            var displayName = memberInfo.GetDisplayName();
                            var description = memberInfo.GetDescription()?.Trim() ?? null;
                            var name = memberInfo.Name;
                            var matchDescription = match.Description.Trim('\n').Trim();
                            Assert.AreEqual(match.Name, displayName);
                            if (description != null) {
                                Assert.AreEqual(matchDescription, description);
                            }
                        } else {
                            throw new Exception($"{enumValue} in {value} is missing");
                        }
                    }
                }
            }
        }
    }
}

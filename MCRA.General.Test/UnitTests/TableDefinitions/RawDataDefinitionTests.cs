using MCRA.Data.Raw.Constants;
using MCRA.General.TableDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TableEnums = MCRA.General.TableDefinitions.RawTableFieldEnums;

namespace MCRA.General.Test.UnitTests.TableDefinitions {
    [TestClass]
    public class RawDataDefinitionTests {

        [TestMethod]
        public void RawDataTableEnumCheckColumnDefinitionsTest() {

            //select one of the types from the RawTableFieldEnums namespace
            var enumType = typeof(TableEnums.RawAgriculturalUses_has_Compounds);
            //get the assembly for this type
            var asm = enumType.Assembly;
            //get all enum types in this assembly from the RawTableFieldEnums namespace
            var fieldEnums = asm.GetTypes().Where(t => t.Namespace == enumType.Namespace);

            var dict = RawTableIdToFieldEnums.EnumToIdMap;

            foreach(var e in fieldEnums) {
                Assert.IsTrue(dict.TryGetValue(e, out RawDataSourceTableID tableID), $"Mapping for {e.Name} not found in dictionary");

                if (tableID != RawDataSourceTableID.Unknown) {
                    //get the table definition
                    var tableDef = McraTableDefinitions.Instance.TableDefinitions[tableID];
                    Assert.IsNotNull(tableDef, $"Table definition for {e.Name} not found.");
                    //get the members of the enum
                    var enumNames = e.GetEnumNames().ToList();

                    var colDefsEnumerator = tableDef.ColumnDefinitions.Where(c => !c.IsDynamic).GetEnumerator();
                    var enumNamesEnumerator = enumNames.GetEnumerator();

                    var index = 0;
                    while (colDefsEnumerator.MoveNext() && enumNamesEnumerator.MoveNext()) {
                        var colDef = colDefsEnumerator.Current;
                        var enumName = enumNamesEnumerator.Current;
                        Assert.AreEqual(enumName, colDef.Id, true, $"Column definition {colDef.Id} doesn't match enum {enumName} at position {index} in table {tableDef.Id}");
                        index++;
                    }
                    if (colDefsEnumerator.MoveNext()) {
                        var colDef = colDefsEnumerator.Current;
                        Assert.Fail($"Column definition {colDef.Id} not found in {e.Name}");
                    }
                    if (enumNamesEnumerator.MoveNext()) {
                        var enumName = enumNamesEnumerator.Current;
                        Assert.Fail($"Enumeration member {enumName} not found in table {tableDef.Id}");
                    }
                }
            }
        }
    }
}

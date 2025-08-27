using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MCRA.Data.Management.DataTemplateGeneration;
using MCRA.Data.Management.Test.Helpers;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.General.TableDefinitions;

namespace MCRA.Data.Management.Test.UnitTests.DataTemplateGeneration {

    [TestClass]
    public class ExcelDatasetTemplateGeneratorTests {
        private static string _outputBasePath = "ExcelDatasetTemplateGeneratorTests";

        [TestMethod]
        public void ExcelDatasetTemplateGenerator_TestCreateAll() {
            var outputFolder = TestUtilities.GetOrCreateTestOutputPath(_outputBasePath);
            var dataTableGroups = Enum.GetValues<SourceTableGroup>()
                .Where(McraModuleDefinitions.Instance.ModuleDefinitionsByTableGroup.ContainsKey);

            var invalidExcelSheetNames = new List<(string Id, string TableName)>();
            foreach (var tableGroup in dataTableGroups) {
                var targetFile = Path.Combine(outputFolder, $"ExcelTemplate_{tableGroup}.xlsx");
                var creator = new ExcelDatasetTemplateGenerator(targetFile);
                creator.Create(tableGroup);
                // Assert file exists
                Assert.IsTrue(File.Exists(targetFile));

                var tableIds = McraTableDefinitions.Instance
                    .GetTableGroupRawTables(tableGroup)
                    .ToArray();

                // Open file and check worksheet names
                var shIdx = 0;

                using (var xlDoc = SpreadsheetDocument.Open(targetFile, false)) {
                    foreach (Sheet sh in xlDoc.WorkbookPart.Workbook.Sheets) {
                        if(shIdx == 0) {
                            Assert.AreEqual("Read me", sh.Name.Value);
                        } else {
                            var tableDef = McraTableDefinitions.Instance.GetTableDefinition(tableIds[shIdx - 1]);
                            var tableName =tableDef.TableName;
                            Assert.AreEqual(tableName, sh.Name.Value);
                            //Excel limits the sheet name to 31 characters max
                            //so test for this here
                            if(tableName.Length > 31) {
                                invalidExcelSheetNames.Add((tableDef.Id, tableName));
                            }
                        }
                        shIdx++;
                    }
                }
            }

            if (invalidExcelSheetNames.Count > 0) {
                var msg = string.Join("\r\n", invalidExcelSheetNames.Select(r => $"Name {r.TableName} (id {r.Id}) is too long ({r.TableName.Length} > 31)."));
                Assert.Fail(msg);
            }
        }
    }
}

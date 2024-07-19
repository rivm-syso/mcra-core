using System.IO.Compression;
using MCRA.Data.Management.DataTemplateGeneration;
using MCRA.General;
using MCRA.General.TableDefinitions;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataTemplateGeneration {

    [TestClass]
    public class CsvDatasetTemplateGeneratorTests {
        private static string _outputBasePath = "CsvDatasetTemplateGeneratorTests";

        [TestMethod]
        [DataRow(SourceTableGroup.Survey)]
        [DataRow(SourceTableGroup.Foods)]
        [DataRow(SourceTableGroup.Effects)]
        [DataRow(SourceTableGroup.SingleValueNonDietaryExposures)]
        [DataRow(SourceTableGroup.Concentrations)]
        [DataRow(SourceTableGroup.AdverseOutcomePathwayNetworks)]
        [DataRow(SourceTableGroup.DeterministicSubstanceConversionFactors)]
        public void CsvDatasetTemplateGenerator_TestCreate(SourceTableGroup tableGroup) {
            var outputFolder = TestUtilities.GetOrCreateTestOutputPath(_outputBasePath);
            var targetFile = Path.Combine(outputFolder, $"CsvTemplate_{tableGroup}.zip");
            var creator = new CsvDatasetTemplateGenerator(targetFile);

            creator.Create(tableGroup);

            // Assert file exists
            Assert.IsTrue(File.Exists(targetFile));

            // Assert csv files and README exist in archive
            var tableDefs = McraTableDefinitions.Instance.GetTableGroupRawTables(tableGroup)
                .Select(r => McraTableDefinitions.Instance.GetTableDefinition(r))
                .ToList();
            using (ZipArchive archive = ZipFile.OpenRead(targetFile)) {
                Assert.IsTrue(archive.Entries.Any(r => r.Name == "README.md"));
                CollectionAssert.IsSubsetOf(
                    tableDefs.Select(r => $"{r.Id}.csv").ToArray(),
                    archive.Entries.Select(r => r.Name).ToArray()
                );
            }
        }
    }
}

using MCRA.Data.Raw.Copying;
using MCRA.Data.Raw.Copying.EuHbmDataCopiers;
using MCRA.General;
using MCRA.Utils.DataFileReading;
using Moq;
using System.Reflection;

namespace MCRA.Data.Raw.Test.UnitTests.Copying {

    [TestClass]
    public class RawDataSourceBulkCopierFactoryTests {

        [TestMethod]
        public void RawDataSourceBulkCopierFactory_CreateTest() {
            var dataSourceReader = new Mock<IDataSourceReader>();
            dataSourceReader.Setup(m => m.GetTableNames()).Returns(() => []);

            var missingGroups = new List<SourceTableGroup>();
            var copiers = RawDataSourceBulkCopierFactory.Create(dataSourceReader.Object, null, null, null);

            var tableGroups = Enum.GetValues(typeof(SourceTableGroup))
                .Cast<SourceTableGroup>()
                .Where(r => r != SourceTableGroup.Unknown)
                .Where(r => r != SourceTableGroup.DietaryExposures)
                .Where(r => r != SourceTableGroup.TargetExposures)
                .Where(r => r != SourceTableGroup.Risks)
                .Where(r => r != SourceTableGroup.FocalFoods)
                .Where(r => r != SourceTableGroup.PbkModelDefinitions)
                .OrderBy(r => r.ToString());

            Assert.AreEqual(copiers.Count, tableGroups.Count());
            Assert.IsFalse(copiers.Any(r => r == null));
            foreach (var tableGroup in tableGroups) {
                var assembly = Assembly.GetAssembly(typeof(RawDataSourceBulkCopierFactory));
                var copierType = assembly.GetType($"MCRA.Data.Raw.Copying.BulkCopiers.{tableGroup}BulkCopier", false, true);
                Assert.IsNotNull(copierType, $"No copier available for {tableGroup}");
            }
        }

        [TestMethod]
        public void RawDataSourceBulkCopierFactory_TestCreateEuHbmImportCopier() {
            var dataSourceReader = new Mock<IDataSourceReader>();
            dataSourceReader
                .Setup(m => m.GetTableNames())
                .Returns(() =>
                    [
                        "STUDYINFO",
                        "SAMPLE",
                        "TIMEPOINT",
                        "SUBJECTUNIQUE",
                        "SUBJECTREPEATED"
                    ]
                );
            var missingGroups = new List<SourceTableGroup>();
            var copiers = RawDataSourceBulkCopierFactory.Create(dataSourceReader.Object, null, null, null);
            CollectionAssert.AreEquivalent(
                copiers.Select(r => r.GetType()).ToArray(),
                new[] { typeof(EuHbmImportDataCopier) }
            );
        }
    }
}

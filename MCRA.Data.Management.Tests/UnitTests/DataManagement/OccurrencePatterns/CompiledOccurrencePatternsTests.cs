using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledOccurrencePatternsTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledOccurrencePatterns_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses")
            );

            var opPatterns = GetAllOccurrencePatterns(managerType);

            CollectionAssert.AreEqual(
                new[] { "au1", "au2", "au3", "au4", "au5", "au6", "au7", "au8" },
                opPatterns.Keys.ToList()
            );
            Assert.AreEqual(0, opPatterns.Sum(a => a.Value.Compounds.Count));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledOccurrencePatterns_TestSimpleFoodsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["F2"]);

            var opPatterns = GetAllOccurrencePatterns(managerType);

            CollectionAssert.AreEqual(new[] { "au3", "au4", "au5" }, opPatterns.Keys.ToList());

            Assert.AreEqual(0, opPatterns.Sum(a => a.Value.Compounds.Count));
            Assert.AreEqual("f2", opPatterns.Values.Select(a => a.Food.Code.ToLower()).Distinct().Single());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledOccurrencePatterns_TestCompoundsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );

            var opPatterns = GetAllOccurrencePatterns(managerType);

            CollectionAssert.AreEqual(
                new[] { "au1", "au2", "au3", "au4", "au5", "au6", "au7", "au8" },
                opPatterns.Keys.ToList()
            );

            var compoundCodes = opPatterns["au1"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, compoundCodes);
            compoundCodes = opPatterns["au2"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C", "D", "E" }, compoundCodes);
            compoundCodes = opPatterns["au3"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            compoundCodes = opPatterns["au4"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "D" }, compoundCodes);
            Assert.IsEmpty(opPatterns["au5"].Compounds);
            compoundCodes = opPatterns["au6"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, compoundCodes);
            compoundCodes = opPatterns["au7"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "E" }, compoundCodes);
            compoundCodes = opPatterns["au8"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);

        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledOccurrencePatterns_TestCompoundsFoodsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2"]);

            var opPatterns = GetAllOccurrencePatterns(managerType);

            CollectionAssert.AreEqual(
                new[] { "au3", "au4", "au5" },
                opPatterns.Keys.ToList()
            );

            var compoundCodes = opPatterns["au3"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            compoundCodes = opPatterns["au4"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "D" }, compoundCodes);
            Assert.IsEmpty(opPatterns["au5"].Compounds);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledOccurrencePatterns_TestCompoundsFoodsCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var opPatterns = GetAllOccurrencePatterns(managerType);

            CollectionAssert.AreEqual(
                new[] { "au3", "au4", "au5" },
                opPatterns.Keys.ToList()
            );

            var compoundCodes = opPatterns["au3"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            Assert.IsEmpty(opPatterns["au4"].Compounds);
            Assert.IsEmpty(opPatterns["au5"].Compounds);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledOccurrencePatterns_TestCompoundsCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var opPatterns = GetAllOccurrencePatterns(managerType);

            CollectionAssert.AreEqual(
                new[] { "au1", "au2", "au3", "au4", "au5", "au6", "au7", "au8" },
                opPatterns.Keys.ToList()
            );

            var compoundCodes = opPatterns["au1"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            compoundCodes = opPatterns["au2"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            compoundCodes = opPatterns["au3"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            Assert.IsEmpty(opPatterns["au4"].Compounds);
            Assert.IsEmpty(opPatterns["au5"].Compounds);
            compoundCodes = opPatterns["au6"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
            Assert.IsEmpty(opPatterns["au7"].Compounds);
            compoundCodes = opPatterns["au8"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
        }
    }
}

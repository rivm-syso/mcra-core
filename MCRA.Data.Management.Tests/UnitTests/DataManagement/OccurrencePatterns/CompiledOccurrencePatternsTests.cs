using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledOccurrencePatternsTests : CompiledTestsBase {
        protected Func<IDictionary<string, OccurrencePattern>> _getItemsDelegate;

        [TestMethod]
        public void CompiledOccurrencePatterns_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses")
            );

            var opPatterns = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(
                new[] { "au1", "au2", "au3", "au4", "au5", "au6", "au7", "au8" },
                opPatterns.Keys.ToList()
            );
            Assert.AreEqual(0, opPatterns.Sum(a => a.Value.Compounds.Count));
        }

        [TestMethod]
        public void CompiledOccurrencePatterns_TestSimpleFoodsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["F2"]);

            var opPatterns = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "au3", "au4", "au5" }, opPatterns.Keys.ToList());

            Assert.AreEqual(0, opPatterns.Sum(a => a.Value.Compounds.Count));
            Assert.AreEqual("f2", opPatterns.Values.Select(a => a.Food.Code.ToLower()).Distinct().Single());
        }

        [TestMethod]
        public void CompiledOccurrencePatterns_TestCompoundsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );

            var opPatterns = _getItemsDelegate.Invoke();

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
            Assert.AreEqual(0, opPatterns["au5"].Compounds.Count);
            compoundCodes = opPatterns["au6"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, compoundCodes);
            compoundCodes = opPatterns["au7"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "E" }, compoundCodes);
            compoundCodes = opPatterns["au8"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);

        }

        [TestMethod]
        public void CompiledOccurrencePatterns_TestCompoundsFoodsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2"]);

            var opPatterns = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(
                new[] { "au3", "au4", "au5" },
                opPatterns.Keys.ToList()
            );

            var compoundCodes = opPatterns["au3"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            compoundCodes = opPatterns["au4"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "A", "D" }, compoundCodes);
            Assert.AreEqual(0, opPatterns["au5"].Compounds.Count);
        }

        [TestMethod]
        public void CompiledOccurrencePatterns_TestCompoundsFoodsCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["f2"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var opPatterns = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(
                new[] { "au3", "au4", "au5" },
                opPatterns.Keys.ToList()
            );

            var compoundCodes = opPatterns["au3"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes);
            Assert.AreEqual(0, opPatterns["au4"].Compounds.Count);
            Assert.AreEqual(0, opPatterns["au5"].Compounds.Count);
        }

        [TestMethod]
        public void CompiledOccurrencePatterns_TestCompoundsCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.OccurrencePatterns, @"AgriculturalUsesTests/AgriculturalUses"),
                (ScopingType.OccurrencePatternsHasCompounds, @"AgriculturalUsesTests/AgriculturalUsesCompounds")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var opPatterns = _getItemsDelegate.Invoke();

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
            Assert.AreEqual(0, opPatterns["au4"].Compounds.Count);
            Assert.AreEqual(0, opPatterns["au5"].Compounds.Count);
            compoundCodes = opPatterns["au6"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
            Assert.AreEqual(0, opPatterns["au7"].Compounds.Count);
            compoundCodes = opPatterns["au8"].Compounds.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "B" }, compoundCodes);
        }
    }
}

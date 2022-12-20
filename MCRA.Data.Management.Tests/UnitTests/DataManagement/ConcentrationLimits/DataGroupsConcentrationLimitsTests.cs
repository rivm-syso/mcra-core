using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement.MaximumResidueLimits {
    [TestClass]
    public class DataGroupsConcentrationLimitsTests : CompiledTestsBase {
        /// <summary>
        /// Tests whether the maximum residue limits table is correctly loaded. Checks for each food and
        /// substance whether the expected MRL is present. Also checks whether no other MRLs are loaded.
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitsDataTest1() {
            _rawDataProvider.SetDataGroupsFromFolder(
                1,
                "_DataGroupsTest",
                new[] { SourceTableGroup.MaximumResidueLimits, SourceTableGroup.Foods, SourceTableGroup.Compounds });
            var foods = _compiledDataManager.GetAllFoods();

            var foodApple = foods["APPLE"];
            var foodBananas = foods["BANANAS"];

            var compounds = _compiledDataManager.GetAllCompounds();
            var compoundA = compounds["CompoundA"];
            var compoundB = compounds["CompoundB"];
            var compoundC = compounds["CompoundC"];
            var compoundD = compounds["CompoundD"];

            var concentrationLimits = _compiledDataManager.GetAllMaximumConcentrationLimits();
            var concentrationLimitsApple = concentrationLimits.Where(m => m.Food == foodApple).ToList();

            Assert.AreEqual(3, concentrationLimitsApple.Count);
            CollectionAssert.Contains(concentrationLimitsApple.Select(m => m.Compound).ToList(), compoundB);
            CollectionAssert.Contains(concentrationLimitsApple.Select(m => m.Compound).ToList(), compoundC);
            CollectionAssert.Contains(concentrationLimitsApple.Select(m => m.Compound).ToList(), compoundA);

            Assert.AreEqual(1, concentrationLimitsApple.Single(m => m.Compound == compoundA).Limit);
            Assert.AreEqual(2, concentrationLimitsApple.Single(m => m.Compound == compoundB).Limit);
            Assert.AreEqual(3, concentrationLimitsApple.Single(m => m.Compound == compoundC).Limit);

            var maximumResidueLimitsBananas = concentrationLimits.Where(m => m.Food == foodBananas).ToList();
            Assert.AreEqual(2, maximumResidueLimitsBananas.Count);
            CollectionAssert.Contains(maximumResidueLimitsBananas.Select(m => m.Compound).ToList(), compoundA);
            CollectionAssert.Contains(maximumResidueLimitsBananas.Select(m => m.Compound).ToList(), compoundD);

            Assert.AreEqual(1, maximumResidueLimitsBananas.Single(m => m.Compound == compoundA).Limit);
            Assert.AreEqual(4, maximumResidueLimitsBananas.Single(m => m.Compound == compoundD).Limit);
        }
    }
}

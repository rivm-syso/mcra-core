using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class DataLinkingAopNetworksTests : LinkTestsBase {
        [TestMethod]
        public void DataLinkingAdverseOutcomePathwayNetworksSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple")
            );

            _compiledLinkManager.LoadScope(SourceTableGroup.AdverseOutcomePathwayNetworks);

            var allReferencedEffects = _compiledLinkManager.GetAllCodes(ScopingType.Effects);
            CollectionAssert.AreEquivalent(new[] { "E1", "E2", "E3", "E4" }, allReferencedEffects.ToArray());

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.AdverseOutcomePathwayNetworks);
            CollectionAssert.AreEqual(new[] { "P1", "P2", "P3", "P4", "P5", "P6" }, scope.ToArray());

            var allSourceEntities = _compiledLinkManager.GetAllSourceEntities(ScopingType.AdverseOutcomePathwayNetworks);
            Assert.HasCount(6, allSourceEntities);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AdverseOutcomePathwayNetworks);

            AssertDataReadingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, 6, "P1,P2,P3,P4,P5,P6", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, ScopingType.Effects, 4, "e1,e2,e3,e4", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "e1,e2,e3,e4");
        }


        [TestMethod]
        public void DataLinkingAdverseOutcomePathwayNetworksSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["E2"]);

            _compiledLinkManager.LoadScope(SourceTableGroup.AdverseOutcomePathwayNetworks);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.AdverseOutcomePathwayNetworks);
            CollectionAssert.AreEqual(new[] { "P3","P4","P5" }, scope.ToArray());

            var allEntities = _compiledLinkManager.GetAllEntities(ScopingType.AdverseOutcomePathwayNetworks);
            Assert.HasCount(6, allEntities);

            var allSourceEntities = _compiledLinkManager.GetAllSourceEntities(ScopingType.AdverseOutcomePathwayNetworks);
            Assert.HasCount(6, allSourceEntities);

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AdverseOutcomePathwayNetworks);
            AssertDataReadingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, 6, "p3,p4,p5", "p1,p2,p6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, ScopingType.Effects, 4, "e2", "e1,e3,e4", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "e2");
        }

        [TestMethod]
        public void DataLinkingEffectRelationsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple"),
                (ScopingType.EffectRelations, @"AdverseOutcomePathwayNetworksTests/EffectRelationsSimple")
            );

            var allReferencedEffects = _compiledLinkManager.GetAllCodes(ScopingType.Effects);
            CollectionAssert.AreEquivalent(new[] { "E1", "E2", "E3", "E4", "E5" }, allReferencedEffects.ToArray());

            var allEffects = _compiledLinkManager.GetAllEntities(ScopingType.Effects);
            CollectionAssert.AreEquivalent(new[] { "E1", "E2", "E3", "E4", "E5" }, allEffects.Keys.ToArray());

            var allReferencedAopNetworks = _compiledLinkManager.GetAllCodes(ScopingType.AdverseOutcomePathwayNetworks);
            CollectionAssert.AreEquivalent(new string[] { "P1", "P2", "P3", "P4", "P5", "P6" }, allReferencedAopNetworks.ToArray());

            _compiledLinkManager.LoadScope(SourceTableGroup.AdverseOutcomePathwayNetworks);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.AdverseOutcomePathwayNetworks);
            CollectionAssert.AreEqual(new[] { "P1", "P2", "P3", "P4", "P5", "P6" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AdverseOutcomePathwayNetworks);
            AssertDataReadingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, 6, "P1,P2,P3,P4,P5,P6", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, ScopingType.Effects, 4, "e1,e2,e3,e4", "", "e5");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRelations, ScopingType.AdverseOutcomePathwayNetworks, 6, "P1,P2,P3,P4,P5,P6", "", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRelations, ScopingType.Effects, 5, "e1,e2,e3,e4,e5", "", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "e1,e2,e3,e4,e5");
        }

        [TestMethod]
        public void DataLinkingEffectRelationsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple"),
                (ScopingType.EffectRelations, @"AdverseOutcomePathwayNetworksTests/EffectRelationsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["E1", "E2"]);

            var allReferencedEffects = _compiledLinkManager.GetAllCodes(ScopingType.Effects);
            CollectionAssert.AreEquivalent(new[] { "E1", "E2" }, allReferencedEffects.ToArray());

            var allReferencedAopNetworks = _compiledLinkManager.GetAllCodes(ScopingType.AdverseOutcomePathwayNetworks);
            CollectionAssert.AreEquivalent(new string[] { "P1", "P2", "P3", "P4", "P5", "P6" }, allReferencedAopNetworks.ToArray());

            _compiledLinkManager.LoadScope(SourceTableGroup.AdverseOutcomePathwayNetworks);

            var scope = _compiledLinkManager.GetCodesInScope(ScopingType.AdverseOutcomePathwayNetworks);
            CollectionAssert.AreEqual(new[] { "P1", "P3", "P4", "P5" }, scope.ToArray());

            var report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.AdverseOutcomePathwayNetworks);
            AssertDataReadingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, 6, "p1,p3,p4,p5", "p2,p6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.AdverseOutcomePathwayNetworks, ScopingType.Effects, 4, "e1,e2", "e3,e4", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRelations, ScopingType.AdverseOutcomePathwayNetworks, 6, "P1,P3,P4,P5", "P2,P6", "");
            AssertDataLinkingSummaryRecord(report, ScopingType.EffectRelations, ScopingType.Effects, 5, "e1,e2", "e3,e4,e5", "");

            report = _compiledLinkManager.GetDataReadingReports(SourceTableGroup.Effects);
            AssertDataReadingSummaryRecord(report, ScopingType.Effects, 0, "", "", "e1,e2");
        }
    }
}

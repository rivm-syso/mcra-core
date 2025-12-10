using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledAopNetworksTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledAdverseOutcomePathwayNetworks_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple")
            );

            var allAops = GetAllAdverseOutcomePathwayNetworks(managerType);

            CollectionAssert.AreEqual(new[] { "P1", "P2", "P3", "P4", "P5", "P6" }, allAops.Keys.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledAdverseOutcomePathwayNetworks_TestSimpleEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple")
            );

            //set a filter scope on effects
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["E2"]);

            var allAops = GetAllAdverseOutcomePathwayNetworks(managerType);

            CollectionAssert.AreEqual(new[] { "P3", "P4", "P5" }, allAops.Keys.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledAdverseOutcomePathwayNetworks_TestCompiledEffectRelationsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple"),
                (ScopingType.EffectRelations, @"AdverseOutcomePathwayNetworksTests/EffectRelationsSimple")
            );

            var allAops = GetAllAdverseOutcomePathwayNetworks(managerType);

            CollectionAssert.AreEqual(new[] { "P1", "P2", "P3", "P4", "P5", "P6" }, allAops.Keys.ToList());
            var ldsafdsaf = allAops["P1"].EffectRelations.SelectMany(r => r.UpstreamKeyEvent.Code).ToList();

            CollectionAssert.AreEqual(new[] { "E1" }, allAops["P1"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E2" }, allAops["P1"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E1", "E2" }, allAops["P2"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E3", "E4" }, allAops["P2"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E2" }, allAops["P3"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E4" }, allAops["P3"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E1", "E1", "E2" }, allAops["P4"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E4", "E2", "E3" }, allAops["P4"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E1", "E2" }, allAops["P5"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E3", "E4" }, allAops["P5"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E3" }, allAops["P6"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E5" }, allAops["P6"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledAdverseOutcomePathwayNetworks_TestCompiledEffectRelationsSimpleEffectsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple"),
                (ScopingType.EffectRelations, @"AdverseOutcomePathwayNetworksTests/EffectRelationsSimple")
            );

            //set a filter scope on effects
            RawDataProvider.SetFilterCodes(ScopingType.Effects, ["E1", "E2"]);

            var allAops = GetAllAdverseOutcomePathwayNetworks(managerType);

            CollectionAssert.AreEqual(new[] { "P1", "P3", "P4", "P5" }, allAops.Keys.ToList());

            Assert.HasCount(1, allAops["P1"].EffectRelations);
            CollectionAssert.AreEqual(new[] { "E1" }, allAops["P1"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E2" }, allAops["P1"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());

            Assert.IsEmpty(allAops["P3"].EffectRelations);

            Assert.HasCount(1, allAops["P4"].EffectRelations);
            CollectionAssert.AreEqual(new[] { "E1" }, allAops["P4"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E2" }, allAops["P4"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());

            Assert.IsEmpty(allAops["P5"].EffectRelations);
        }
    }
}

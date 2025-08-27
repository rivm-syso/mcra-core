using MCRA.General;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledAopNetworksTests : CompiledTestsBase {
        protected Func<IDictionary<string, AdverseOutcomePathwayNetwork>> _getItemsDelegate;

        [TestMethod]
        public void CompiledAdverseOutcomePathwayNetworks_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple")
            );

            var allAops = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "P1", "P2", "P3", "P4", "P5", "P6" }, allAops.Keys.ToList());
        }

        [TestMethod]
        public void CompiledAdverseOutcomePathwayNetworks_TestSimpleEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple")
            );

            //set a filter scope on effects
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["E2"]);

            var allAops = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "P3", "P4", "P5" }, allAops.Keys.ToList());
        }

        [TestMethod]
        public void CompiledAdverseOutcomePathwayNetworks_TestCompiledEffectRelationsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple"),
                (ScopingType.EffectRelations, @"AdverseOutcomePathwayNetworksTests/EffectRelationsSimple")
            );

            var allAops = _getItemsDelegate.Invoke();

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
        public void CompiledAdverseOutcomePathwayNetworks_TestCompiledEffectRelationsSimpleEffectsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.AdverseOutcomePathwayNetworks, @"AdverseOutcomePathwayNetworksTests/AopNetworksSimple"),
                (ScopingType.EffectRelations, @"AdverseOutcomePathwayNetworksTests/EffectRelationsSimple")
            );

            //set a filter scope on effects
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["E1", "E2"]);

            var allAops = _getItemsDelegate.Invoke();

            CollectionAssert.AreEqual(new[] { "P1", "P3", "P4", "P5" }, allAops.Keys.ToList());

            Assert.AreEqual(1, allAops["P1"].EffectRelations.Count);
            CollectionAssert.AreEqual(new[] { "E1" }, allAops["P1"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E2" }, allAops["P1"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());

            Assert.AreEqual(0, allAops["P3"].EffectRelations.Count);

            Assert.AreEqual(1, allAops["P4"].EffectRelations.Count);
            CollectionAssert.AreEqual(new[] { "E1" }, allAops["P4"].EffectRelations.Select(r => r.UpstreamKeyEvent.Code).ToList());
            CollectionAssert.AreEqual(new[] { "E2" }, allAops["P4"].EffectRelations.Select(r => r.DownstreamKeyEvent.Code).ToList());

            Assert.AreEqual(0, allAops["P5"].EffectRelations.Count);
        }
    }
}

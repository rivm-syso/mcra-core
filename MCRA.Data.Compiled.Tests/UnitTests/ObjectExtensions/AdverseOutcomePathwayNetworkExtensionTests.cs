using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Test {
    [TestClass]
    public class AdverseOutcomePathwayNetworkExtensionsTests {

        [TestMethod]
        public void AdverseOutcomePathwayNetworkExtensions_TestGetAllEffects() {
            var fake = FakeAopNetwork;
            var allEffects = fake.GetAllEffects();
            Assert.AreEqual(8, allEffects.Count);
        }

        [TestMethod]
        public void AdverseOutcomePathwayNetworkExtensions_TestGetSubNetwork() {
            var fake = FakeAopNetwork;
            var allEffects = fake.GetAllEffects().ToDictionary(r => r.Code);
            var subNetwork = fake.GetSubNetwork(allEffects["KE1B"], allEffects["KE1A"]);
            var subNetworkEffects = subNetwork.GetAllEffects();
            CollectionAssert.AreEquivalent(
                new[] { "MIE1", "MIE3", "KE1A", "KE1B" },
                subNetworkEffects.Select(r => r.Code).ToArray()
            );
            Assert.AreEqual(4, subNetwork.GetAllEffects().Count);
        }

        #region Fakes

        private AdverseOutcomePathwayNetwork FakeAopNetwork {
            get {
                var aopNetwork = new AdverseOutcomePathwayNetwork();
                var kes = new[] {
                    createEffect("MIE1", "Molecular initiating event 1", BiologicalOrganisationType.Molecular),
                    createEffect("MIE2", "Molecular initiating event 2", BiologicalOrganisationType.Molecular),
                    createEffect("MIE3", "Molecular initiating event 3", BiologicalOrganisationType.Molecular),
                    createEffect("MIE4", "Molecular initiating event 4", BiologicalOrganisationType.Molecular),
                    createEffect("KE1A", "Key event 1A", BiologicalOrganisationType.Cellular),
                    createEffect("KE1B", "Key event 1B", BiologicalOrganisationType.Organ),
                    createEffect("KE2", "Key event 2", BiologicalOrganisationType.Organ),
                    createEffect("AO", "Adverse Outcome", BiologicalOrganisationType.Individual),
                }.ToDictionary(r => r.Code);
                var kers = createEdges(aopNetwork, kes, [
                    ("MIE1", "KE1A"),
                    ("KE1A", "KE1B"),
                    ("MIE2", "KE2"),
                    ("MIE3", "KE1A"),
                    ("MIE3", "KE2"),
                    ("MIE4", "KE2"),
                    ("MIE4", "KE1B"),
                    ("KE1B", "AO"),
                    ("KE2", "AO")
                ]);
                aopNetwork.AdverseOutcome = kes["AO"];
                aopNetwork.EffectRelations = kers;
                return aopNetwork;
            }
        }

        private static Effect createEffect(
            string code,
            string name,
            BiologicalOrganisationType biologicalOrganisation
        ) {
            return new Effect() {
                Code = code,
                Name = name,
                BiologicalOrganisationType = biologicalOrganisation
            };
        }

        private static List<EffectRelationship> createEdges(
            AdverseOutcomePathwayNetwork aopNetwork,
            IDictionary<string, Effect> nodes,
            (string Upstream, string Downstream)[] edges
        ) {
            return edges
                .Select(r => new EffectRelationship() {
                    AdverseOutcomePathwayNetwork = aopNetwork,
                    UpstreamKeyEvent = nodes[r.Upstream],
                    DownstreamKeyEvent = nodes[r.Downstream]
                })
                .ToList();
        }

        #endregion

    }
}

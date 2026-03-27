using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Test {
    [TestClass]
    public class AdverseOutcomePathwayNetworkExtensionsTests {

        [TestMethod]
        public void AdverseOutcomePathwayNetworkExtensions_TestGetAllEffects() {
            var fake = FakeSimpleAopNetwork;
            var allEffects = fake.GetAllEffects();
            Assert.HasCount(8, allEffects);
        }

        [TestMethod]
        [DataRow("simple", "KE1B", "KE1A", new string[] { "MIE1", "MIE3", "KE1A", "KE1B" })]
        public void AdverseOutcomePathwayNetworkExtensions_TestGetSubNetwork(
            string id,
            string idAdverseOutcome,
            string idCommonKeyEvent,
            string[] expected
        ) {
            var fake = GetFakeAopNetwork(id);
            var allEffects = fake.GetAllEffects().ToDictionary(r => r.Code);
            var subNetwork = fake.GetSubNetwork(allEffects[idAdverseOutcome], allEffects[idCommonKeyEvent]);
            var subNetworkEffects = subNetwork.GetAllEffects();
            CollectionAssert.AreEquivalent(
                expected,
                subNetworkEffects.Select(r => r.Code).ToArray()
            );
            Assert.HasCount(4, subNetwork.GetAllEffects());
        }

        [TestMethod]
        [DataRow("circular", "KE1", new string[] { "KE1", "KE2", "AO" })]
        public void AdverseOutcomePathwayNetworkExtensions_TestGetDownstreamEffectRelations(
            string id,
            string idKeyEvent,
            string[] expected
        ) {
            var fake = GetFakeAopNetwork(id);
            var allEffects = fake.GetAllEffects().ToDictionary(r => r.Code);
            var result = fake.GetDownstreamEffectRelations(allEffects[idKeyEvent]);
            var actual = result.Select(r => r.DownstreamKeyEvent.Code).ToArray();
            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestMethod]
        [DataRow("circular", "KE1", new string[] { "KE1", "KE2", "MIE1", "MIE2" })]
        public void AdverseOutcomePathwayNetworkExtensions_TestGetUpstreamEffectRelations(
            string id,
            string idKeyEvent,
            string[] expected
        ) {
            var fake = GetFakeAopNetwork(id);
            var allEffects = fake.GetAllEffects().ToDictionary(r => r.Code);
            var result = fake.GetUpstreamEffectRelations(allEffects[idKeyEvent]);
            var actual = result.Select(r => r.UpstreamKeyEvent.Code).ToArray();
            CollectionAssert.AreEquivalent(expected, actual);
        }

        [TestMethod]
        [DataRow("circular", "KE1", new string[] { "KE1", "KE2" })]
        public void AdverseOutcomePathwayNetworkExtensions_TestFindFeedbackRelationships(
            string id,
            string idKeyEvent,
            string[] expected
        ) {
            var fake = GetFakeAopNetwork(id);
            var result = fake.FindFeedbackRelationships();
            var actual = result
                .Select(r => r.UpstreamKeyEvent.Code)
                .Union(result.Select(r => r.DownstreamKeyEvent.Code))
                .ToArray();
            CollectionAssert.AreEquivalent(expected, actual);
        }

        #region Fakes

        private static AdverseOutcomePathwayNetwork GetFakeAopNetwork(string id) {
            switch (id) {
                case "simple":
                    return FakeSimpleAopNetwork;
                case "circular":
                    return FakeCircularAopNetwork;
                default:
                    throw new NotImplementedException();
            }
        }

        private static AdverseOutcomePathwayNetwork FakeCircularAopNetwork {
            get {
                var aopNetwork = new AdverseOutcomePathwayNetwork();
                var kes = new[] {
                    createEffect("MIE1", "Molecular initiating event 1", BiologicalOrganisationType.Molecular),
                    createEffect("MIE2", "Molecular initiating event 2", BiologicalOrganisationType.Molecular),
                    createEffect("KE1", "Key event 1", BiologicalOrganisationType.Cellular),
                    createEffect("KE2", "Key event 1", BiologicalOrganisationType.Organ),
                    createEffect("AO", "Adverse Outcome", BiologicalOrganisationType.Individual),
                }.ToDictionary(r => r.Code);
                var kers = createEdges(aopNetwork, kes, [
                    ("MIE1", "KE1"),
                    ("MIE2", "KE1"),
                    ("KE1", "KE2"),
                    ("KE2", "KE1"),
                    ("KE2", "AO")
                ]);
                aopNetwork.AdverseOutcome = kes["AO"];
                aopNetwork.EffectRelations = kers;
                return aopNetwork;
            }
        }

        private static AdverseOutcomePathwayNetwork FakeSimpleAopNetwork {
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

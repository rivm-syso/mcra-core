using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock absorption factors
    /// </summary>
    public static class MockAdverseOutcomePathwayNetworkGenerator {

        public static AdverseOutcomePathwayNetwork SimpleFake {
            get {
                var aopNetwork = new AdverseOutcomePathwayNetwork() {
                    Code = "FakeAopNetwork",
                    Name = "Fake AOP Network",
                    Description = "Fake AOP Network.",
                    Reference = "ref",
                    RiskTypeString = "unknown"
                };

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
                var kers = createEdges(aopNetwork, kes, new[] {
                        ("MIE1", "KE1A"),
                        ("KE1A", "KE1B"),
                        ("MIE2", "KE2"),
                        ("MIE3", "KE1A"),
                        ("MIE3", "KE2"),
                        ("MIE4", "KE2"),
                        ("MIE4", "KE1B"),
                        ("KE1B", "AO"),
                        ("KE2", "AO")
                    });
                aopNetwork.AdverseOutcome = kes["AO"];
                aopNetwork.EffectRelations = kers;
                return aopNetwork;
            }
        }

        public static AdverseOutcomePathwayNetwork CyclicFake {
            get {
                var aopNetwork = SimpleFake;
                var effects = aopNetwork.GetAllEffects().ToDictionary(r => r.Code);
                aopNetwork.EffectRelations.Add(new EffectRelationship() {
                    AdverseOutcomePathwayNetwork = aopNetwork,
                    UpstreamKeyEvent = effects["KE1B"],
                    DownstreamKeyEvent = effects["KE1A"]
                });
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
    }
}

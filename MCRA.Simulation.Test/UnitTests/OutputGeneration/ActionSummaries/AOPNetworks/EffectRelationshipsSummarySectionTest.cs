using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.AOPNetworks {
    /// <summary>
    /// OutputGeneration, ActionSummaries, AOPNetworks
    /// </summary>
    [TestClass]
    public class EffectRelationshipsSummarySectionTests: SectionTestBase
    {
        /// <summary>
        /// Summarize, test EffectRelationshipsSummarySection view
        /// </summary>
        [TestMethod]
        public void EffectRelationshipsSummarySection_Test() {
            var effects = FakeEffectsGenerator.Create(5);

            var effectRelations0 = new List<EffectRelationship>();
            var adverseOutcomePathwayNetwork0 = new AdverseOutcomePathwayNetwork() {
                AdverseOutcome = effects[0],
                EffectRelations = effectRelations0,
                Code = "AdverseOutcomePathwayNetworkCode",
                Name = "AdverseOutcomePathwayNetworkName",
                Description = "AdverseOutcomePathwayNetworkDescription",
                Reference = "ref",
                RiskTypeString = "unknown"
            };

            effectRelations0.Add(new EffectRelationship() {
                AdverseOutcomePathwayNetwork = adverseOutcomePathwayNetwork0,
                DownstreamKeyEvent = effects[0],
                UpstreamKeyEvent = effects[1],
            });
            var section = new EffectRelationshipsSummarySection();
            section.Summarize(adverseOutcomePathwayNetwork0, effects);
            Assert.AreEqual(5, section.Records.Count);
            AssertIsValidView(section);
        }
    }
}

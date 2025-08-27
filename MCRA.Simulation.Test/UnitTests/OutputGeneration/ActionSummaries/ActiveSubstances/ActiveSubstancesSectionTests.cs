using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ActiveSubstances {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ActiveSubstances
    /// </summary>
    [TestClass]
    public class ActiveSubstancesSectionsTests {
        /// <summary>
        /// Summarize
        /// </summary>
        [TestMethod]
        public void ActiveSubstances_Test1() {
            var substances = FakeSubstancesGenerator.Create(3);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var model = new ActiveSubstanceModel() {
                Code = "AssessmentGroupMembershipModelCode",
                Name = "AssessmentGroupMembershipModelName",
                Description = "AssessmentGroupMembershipModelDescription",
                Effect = new Effect() {
                    Code = "effectCode",
                    Name = "effectName"
                },
                Reference = "Reference",
                Accuracy = 1d,
                Sensitivity = 1d,
                Specificity = 1,
                MembershipProbabilities = memberships,
            };
            var section = new ActiveSubstancesSummarySection();
            section.Summarize(memberships, substances, model.Effect);
            Assert.AreEqual(1, section.Records.Count);
        }
    }
}

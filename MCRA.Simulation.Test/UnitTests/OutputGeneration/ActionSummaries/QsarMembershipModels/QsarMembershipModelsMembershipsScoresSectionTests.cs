using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.QsarMembershipModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, QsarMembershipModels
    /// </summary>
    [TestClass]
    public class QsarMembershipModelsMembershipsScoresSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test QsarMembershipModelsMembershipScoresSection view
        /// </summary>
        [TestMethod]
        public void QsarMembershipModelsMembershipsScoresSection_Test1() {
            var section = new QsarMembershipModelsMembershipScoresSection() {
                Records = new List<QsarMembershipModelMembershipScoresRecord>() {
                    new QsarMembershipModelMembershipScoresRecord() {
                        Code = "QSAR" ,
                        Name = "QSAR",
                        MembershipScores = new List<QsarMembershipModelSubstanceRecord>(),
                    }
                }
            };
            section.Summarize(new List<QsarMembershipModel>(), new HashSet<Compound>());
            AssertIsValidView(section);
        }
    }
}
using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Actions.SubstanceApprovals;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the SubstanceApprovals action
    /// </summary>
    [TestClass]
    public class SubstanceApprovalsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the SubstanceApprovals action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void SubstanceApprovalsActionCalculator_TestLoadAndSummarize() {
            // Arrange
            var substances = MockSubstancesGenerator.Create(9);
            var compiledData = new CompiledData() {
                AllSubstanceApprovals = MockSubstanceApprovalsGenerator.Create(substances),
            };
            var project = new ProjectDto();
            var data = new ActionData();
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new SubstanceApprovalsActionCalculator(project);

            // Act
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, $"SubstanceApprovals");

            // Assert
            Assert.IsNotNull(data.SubstanceApprovals);
            Assert.AreEqual(9, data.SubstanceApprovals.Count);
            var approvalsIn = compiledData.AllSubstanceApprovals.Select(f => f.IsApproved).ToList();
            var approvalsOut = data.SubstanceApprovals.Select(f => f.Value.IsApproved).ToList();
            CollectionAssert.AreEquivalent(approvalsIn, approvalsOut);
            WriteReport(header, "TestLoadAndSummarize.html");
        }
    }
}

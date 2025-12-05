using MCRA.Data.Compiled;
using MCRA.Data.Management;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.Substances;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the Substances action
    /// </summary>
    [TestClass]
    public class SubstancesActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Tests load data of substances action calculator. Expect one substance
        /// and reference substance not null on single substance analysis.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestSingle() {
            var substances = FakeSubstancesGenerator.Create(1);
            var compiledData = new CompiledData() {
                AllSubstances = substances.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            var config = project.SubstancesSettings;
            config.MultipleSubstances = false;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.HasCount(1, data.AllCompounds);
            Assert.IsNull(data.ReferenceSubstance);
        }

        /// <summary>
        /// Tests load data of substances action calculator. Expect exception on single
        /// substance analysis with multiple substances.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestSingleFailOnMultipleSubstances() {
            var substances = FakeSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllSubstances = substances.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            var config = project.SubstancesSettings;
            config.MultipleSubstances = false;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            Assert.ThrowsExactly<Exception>(() => TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad"));
        }

        /// <summary>
        /// Tests load data of substances action calculator. Expect multiple substance
        /// and reference substance being null for non-cumulative multiple substance
        /// analysis.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestMultipleNotCumulative() {
            var substances = FakeSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllSubstances = substances.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            var config = project.ConcentrationModelsSettings;
            config.MultipleSubstances = true;
            config.Cumulative = false;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.HasCount(3, data.AllCompounds);
            Assert.IsNull(data.ReferenceSubstance);
        }

        /// <summary>
        /// Tests load data of substances action calculator. Test multiple substances
        /// and cumulative. Fail when no reference is specified.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestCumulativeFailNoReference() {
            var substances = FakeSubstancesGenerator.Create(3);
            var compiledData = new CompiledData() {
                AllSubstances = substances.ToDictionary(c => c.Code)
            };

            var project = new ProjectDto();
            var config = project.ConcentrationModelsSettings;
            config.MultipleSubstances = true;
            config.Cumulative = true;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            // We should not get here, but if we do, this is what we expect
            Assert.HasCount(3, data.AllCompounds);
            Assert.IsNull(data.ReferenceSubstance);
        }

        /// <summary>
        /// Tests load data of substances action calculator. Test single substance
        /// selected on multiple substances analysis. Expect reference substance
        /// not null and equal to the one substance.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestSingleAsMultiple() {
            var substances = FakeSubstancesGenerator.Create(1)
                .ToDictionary(c => c.Code);
            var compiledData = new CompiledData() {
                AllSubstances = substances
            };

            var project = new ProjectDto();
            var config = project.ConcentrationModelsSettings;
            config.MultipleSubstances = true;
            config.Cumulative = false;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.HasCount(1, data.AllCompounds);
            Assert.IsNull(data.ReferenceSubstance);
        }

        /// <summary>
        /// Tests load data of substances action calculator. Test cumulative multiple
        /// substances. Expect multiple substances loaded and reference substance
        /// selected.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestMultipleCumulative() {
            var substances = FakeSubstancesGenerator.Create(3).ToDictionary(c => c.Code);
            var compiledData = new CompiledData() {
                AllSubstances = substances
            };
            var project = new ProjectDto();
            var config = project.SubstancesSettings;
            config.MultipleSubstances = true;
            project.ConcentrationModelsSettings.Cumulative = true;
            config.CodeReferenceSubstance = substances.First().Key;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            Assert.IsNotNull(data.AllCompounds);
            Assert.IsNull(data.ReferenceSubstance);
        }

        /// <summary>
        /// Tests load data of substances action calculator. Test cumulative multiple
        /// substances with unspecified reference substance. Expect exception.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestMultipleCumulativeFailNoReference() {
            var substances = FakeSubstancesGenerator.Create(3).ToDictionary(c => c.Code);
            var compiledData = new CompiledData() {
                AllSubstances = substances
            };
            var project = new ProjectDto();
            var config = project.ConcentrationModelsSettings;
            config.MultipleSubstances = true;
            config.Cumulative = true;

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            // We should not get here, but if we do, this is what we expect
            Assert.IsNotNull(data.AllCompounds);
            Assert.IsNull(data.ReferenceSubstance);
        }

        /// <summary>
        /// Tests load data of substances action calculator. Test cumulative multiple
        /// substances with non-available reference substance. Expect exception.
        /// </summary>
        [TestMethod]
        public void SubstancesActionCalculator_TestMultipleCumulativeFailIncorrectReference() {
            var substances = FakeSubstancesGenerator.Create(3).ToDictionary(c => c.Code);
            var compiledData = new CompiledData() {
                AllSubstances = substances
            };
            var project = new ProjectDto();
            var config = project.SubstancesSettings;
            config.MultipleSubstances = true;
            project.ConcentrationModelsSettings.Cumulative = true;
            config.CodeReferenceSubstance = "XXX";

            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var data = new ActionData();
            var calculator = new SubstancesActionCalculator(project);

            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");

            // We should not get here, but if we do, this is what we expect
            Assert.IsNotNull(data.AllCompounds);
            Assert.IsNull(data.ReferenceSubstance);
        }
    }
}
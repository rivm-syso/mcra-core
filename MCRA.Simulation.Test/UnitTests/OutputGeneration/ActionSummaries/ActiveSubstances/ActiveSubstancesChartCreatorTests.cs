using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ActiveSubstances {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ActiveSubstances
    /// </summary>
    [TestClass]
    public class ActiveSubstancesChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesChartCreator_TestCreate1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var model = new ActiveSubstanceModelRecord() {
                Name = "Model",
                Code = "Model",
                MembershipProbabilities = substances.Select(r => new ActiveSubstanceRecord() {
                    SubstanceCode = r.Code,
                    SubstanceName = r.Name,
                    Probability = ContinuousUniformDistribution.Draw(random, 0, 1),
                }).ToList()
            };
            var section = new ActiveSubstancesSummarySection() {
                Records = [model],
            };
            var chart = new ActiveSubstancesChartCreator(section);
            RenderChart(chart, "TestCreate1");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesChartCreator_TestCreate2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = Enumerable.Range(1, 20).Select(r => $"Compound {r}").ToList();
            var model = new ActiveSubstanceModelRecord() {
                Name = "Model",
                Code = "Model",
                MembershipProbabilities = substances.Select(r => new ActiveSubstanceRecord() {
                    SubstanceCode = r,
                    SubstanceName = r,
                    Probability = ContinuousUniformDistribution.Draw(random, 0, 1),
                }).ToList()
            };
            var section = new ActiveSubstancesSummarySection() {
                Records = [model],
            };
            var chart = new ActiveSubstancesChartCreator(section);
            RenderChart(chart, "TestCreate2");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesChartCreator_TestCreate3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = Enumerable.Range(1, 100).Select(r => $"Compound {r}").ToList();

            var model = new ActiveSubstanceModelRecord() {
                Name = "Model",
                Code = "Model",
                MembershipProbabilities = substances.Select(r => new ActiveSubstanceRecord() {
                    SubstanceCode = r,
                    SubstanceName = r,
                    Probability = ContinuousUniformDistribution.Draw(random, 0, 1),
                }).ToList()
            };

            var section = new ActiveSubstancesSummarySection() {
                Records = [model],
            };

            var chart = new ActiveSubstancesChartCreator(section);
            RenderChart(chart, "TestCreate3");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesChartCreator_TestCreate4() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = Enumerable.Range(1, 100).Select(r => $"Compound {r}").ToList();

            var model = new ActiveSubstanceModelRecord() {
                Name = "Model",
                Code = "Model",
                MembershipProbabilities = substances.Select(r => new ActiveSubstanceRecord() {
                    SubstanceCode = r,
                    SubstanceName = r,
                    Probability = DiscreteUniformDistribution.Draw(random, 0, 1)
                }).ToList()
            };

            var section = new ActiveSubstancesSummarySection() {
                Records = [model],
            };

            var chart = new ActiveSubstancesChartCreator(section);
            RenderChart(chart, "TestCreate4");
        }
    }
}

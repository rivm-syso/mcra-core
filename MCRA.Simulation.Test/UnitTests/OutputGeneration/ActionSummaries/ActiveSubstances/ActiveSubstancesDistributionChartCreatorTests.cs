using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ActiveSubstances {

    /// <summary>
    /// OutputGeneration, ActionSummaries, ActiveSubstances
    /// </summary>
    [TestClass]
    public class ActiveSubstancesDistributionChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesDistributionChartCreator_TestCreate1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = Enumerable.Range(1, 1000).Select(r => $"Compound {r}").ToList();

            var model = new ActiveSubstanceModelRecord() {
                Name = "Model",
                Code = "Model",
                MembershipProbabilities = substances.Select(r => new ActiveSubstanceRecord() {
                    SubstanceCode = r,
                    SubstanceName = r,
                    Probability = ContinuousUniformDistribution.Draw(random, 0, 1)
                }).ToList()
            };

            var section = new ActiveSubstancesSummarySection() {
                Records = new List<ActiveSubstanceModelRecord>() { model },
            };

            var chart = new ActiveSubstancesDistributionChartCreator(section);
            RenderChart(chart, "TestCreate1");
        }

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesDistributionChartCreator_TestCreate2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = Enumerable.Range(1, 1000).Select(r => $"Compound {r}").ToList();

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
                Records = new List<ActiveSubstanceModelRecord>() { model },
            };

            var chart = new ActiveSubstancesDistributionChartCreator(section);
            RenderChart(chart, "TestCreate2");
        }
    }
}

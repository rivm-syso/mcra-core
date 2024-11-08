using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries,DietaryExposures, IntakeModels, ModelThenAdd
    /// </summary>
    [TestClass]
    public class MtaDistributionByFoodsAsMeasuredChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create total exposure distribution by food chart.
        /// </summary>
        [TestMethod]
        public void MtaDistributionByFoodsAsMeasuredChartCreator_TestCreate() {
            int seed = 1;
            (var section, var _) = createFakeModelThenAddDataSection(20, 500, seed);

            var chart = new MtaDistributionByFoodAsMeasuredChartCreator(section, "mg/kg bw/day", false);
            RenderChart(chart, $"TestCreate");
        }

        /// <summary>
        /// Create contributions to total exposure distribution by food chart.
        /// </summary>
        [TestMethod]
        public void MtaDistributionByFoodsAsMeasuredChartCreator_TestCreateContributions() {
            int seed = 1;
            (var section, var _) = createFakeModelThenAddDataSection(20, 500, seed);

            var chart = new MtaDistributionByFoodAsMeasuredChartCreator(section, "mg/kg bw/day", true);
            RenderChart(chart, $"TestCreateContributions");
        }

        private static (UsualIntakeDistributionPerFoodAsMeasuredSection section, List<Food> foods) createFakeModelThenAddDataSection(
            int numberOfFoods,
            int numberOfIndividuals,
            int seed = 1
        ) {
            var foods = FakeFoodsGenerator.Create(numberOfFoods);
            var categories = foods.Select(r => new Category(r.Code, r.Name)).ToList();
            var individualExposuresByCategory = MtaFakeDataGenerator
                .CreateFakeIndividualExposuresByCategory(numberOfIndividuals, categories, seed);

            var section = new UsualIntakeDistributionPerFoodAsMeasuredSection() {
                Categories = categories,
                IndividualExposuresByCategory = individualExposuresByCategory,
            };
            return (section, foods);
        }
    }
}

using MCRA.Utils.Xml;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ModelThenAdd
    /// </summary>
    [TestClass]
    public class MtaFrontendChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Test creation of all MTA charts relevant for the MTA frontend form.
        /// </summary>
        [TestMethod]
        public void MtaFrontendChartCreatorTests_TestCreateAll() {

            // Create a fake intakes by food section (should be fetched from an output in production scenarios)
            int seed = 1;
            var section = createFakeModelThenAddDataSection(20, 500, seed);

            // Create fake foods representing the foods of our current MTA project
            var categories = section.Categories;

            // Create intake models per category (should come from project settings)
            var intakeModelsPerCategory = new List<IntakeModelPerCategory>() {
                new() {
                    FoodsAsMeasured = categories.Take(1).Select(r => r.Id).ToList(),
                    ModelType = IntakeModelType.BBN,
                    TransformType = TransformType.Logarithmic
                },
                new() {
                    FoodsAsMeasured = categories.Skip(1).Take(2).Select(r => r.Id).ToList(),
                    ModelType = IntakeModelType.LNN0,
                    TransformType = TransformType.Logarithmic
                },
                new() {
                    FoodsAsMeasured = ["unknown food"],
                    ModelType = IntakeModelType.LNN,
                    TransformType = TransformType.Logarithmic
                },
            };

            // Total exposure distribution chart (with contributions by food)
            var chartCreator = new MtaDistributionByFoodAsMeasuredChartCreator(section, null, false);
            RenderChart(chartCreator, $"TestCreateTotal");

            // Contributions total exposure distribution by food chart
            chartCreator = new MtaDistributionByFoodAsMeasuredChartCreator(section, null, true);
            RenderChart(chartCreator, $"TestCreateTotalContributions");

            for (int i = 0; i < intakeModelsPerCategory.Count; i++) {
                var model = intakeModelsPerCategory[i];
                var name = $"Model {i}";

                // Group exposure distribution chart with contributions by food
                var modelChartCreator = new MtaGroupingDistributionByFoodAsMeasuredChartCreator(section, model, name, null, false);
                RenderChart(modelChartCreator, $"Test-{name}");
            }

            // Others/remaining category exposure distribution chart with contributions by food
            chartCreator = new MtaOthersDistributionByFoodAsMeasuredChartCreator(section, intakeModelsPerCategory, null, false);
            RenderChart(chartCreator, $"TestCreateOthers");

            // Contributions to others/remaining exposure distribution by food chart
            chartCreator = new MtaOthersDistributionByFoodAsMeasuredChartCreator(section, intakeModelsPerCategory, null, true);
            RenderChart(chartCreator, $"TestCreateOthers");
        }

        /// <summary>
        /// Test deserialization of MTA output section xml. Test to assure backward compatibility
        /// when changes are made to the section.
        /// </summary>
        [TestMethod]
        public void MtaFrontendChartCreatorTests_TestDeserialize() {
            var sectionXml = fakeOutputSectionXml();
            var section = XmlSerialization.FromXml<UsualIntakeDistributionPerFoodAsMeasuredSection>(sectionXml);
            Assert.IsNotNull(section);
            Assert.HasCount(2, section.IndividualExposuresByCategory);
            Assert.HasCount(3, section.Categories);
        }

        private static UsualIntakeDistributionPerFoodAsMeasuredSection createFakeModelThenAddDataSection(
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
            return section;
        }

        private string fakeOutputSectionXml() {
            return @"
            <IndividualExposureDistributionsByCategory>
                <SectionId>9f55dcd2-bcf0-467d-a898-3fecec1afcdc</SectionId>
                <IndividualExposuresByCategory>
                    <CategorizedIndividualExposure>
                        <SimulatedIndividualId>0</SimulatedIndividualId>
                        <SamplingWeight>4.64</SamplingWeight>
                        <CategoryExposures>
                            <CategoryExposure>
                                <IdCategory>Fig</IdCategory>
                                <Exposure>5.48</Exposure>
                            </CategoryExposure>
                            <CategoryExposure>
                                <IdCategory>Grapefruit</IdCategory>
                                <Exposure>4.45</Exposure>
                            </CategoryExposure>
                        </CategoryExposures>
                    </CategorizedIndividualExposure>
                    <CategorizedIndividualExposure>
                        <SimulatedIndividualId>1</SimulatedIndividualId>
                        <SamplingWeight>4.45</SamplingWeight>
                        <CategoryExposures>
                            <CategoryExposure>
                                <IdCategory>Fig</IdCategory>
                                <Exposure>2.99</Exposure>
                            </CategoryExposure>
                            <CategoryExposure>
                                <IdCategory>Imbe</IdCategory>
                                <Exposure>6.20</Exposure>
                            </CategoryExposure>
                        </CategoryExposures>
                    </CategorizedIndividualExposure>
                </IndividualExposuresByCategory>
                <Categories>
                    <Category>
                        <Id>Fig</Id>
                        <Name>Fig</Name>
                    </Category>
                    <Category>
                        <Id>Grapefruit</Id>
                        <Name>Grapefruit</Name>
                    </Category>
                    <Category>
                        <Id>Hackberry</Id>
                        <Name>Hackberry</Name>
                    </Category>
                </Categories>
            </IndividualExposureDistributionsByCategory>
            ";
        }
    }
}

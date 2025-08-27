using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.ConsumerProductExposures;
using MCRA.Simulation.Calculators.ConsumerProductConcentrationModelCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions.ConsumerProductExposures {
    /// <summary>
    /// Runs the ConsumerProductExposures action
    /// </summary>
    [TestClass]
    public class ConsumerProductExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ConsumerProductExposures action: simulate individuals
        /// </summary>
        [TestMethod]
        public void ConsumerProductExposuresActionCalculator_TestSimulate() {
            var seed = 1;
            var numberOfIndividuals = 10;
            var random = new McraRandomGenerator(seed);

            var substances = FakeSubstancesGenerator.Create(1);
            var products = FakeConsumerProductsGenerator.Create(5);
            var selectedPopulation = FakePopulationsGenerator.Create(1).First();

            var populationIndividualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue>();
            var properties = FakeIndividualPropertiesGenerator.Create();
            var propertyValueAge = new PopulationIndividualPropertyValue {
                IndividualProperty = properties[1]
            };
            populationIndividualPropertyValues["Age"] = propertyValueAge;
            var propertyValueSex = new PopulationIndividualPropertyValue {
                IndividualProperty = properties[0],
                Value = string.Join(",", properties[0].CategoricalLevels)
            };
            populationIndividualPropertyValues["Sex"] = propertyValueSex;
            var propertyValueBsa = new PopulationIndividualPropertyValue() {
                IndividualProperty = FakeIndividualPropertiesGenerator.CreateFake(
                    "BSA",
                    IndividualPropertyType.Nonnegative
                    )
            };
            populationIndividualPropertyValues["BSA"] = propertyValueBsa;

            selectedPopulation.PopulationIndividualPropertyValues = populationIndividualPropertyValues;

            var cpConcentrations = FakeConsumerProductConcentrationsGenerator.Create(substances, products);
            var project = new ProjectDto();
            var config = project.ConsumerProductExposuresSettings;
            config.SelectedExposureRoutes = [ExposureRoute.Inhalation, ExposureRoute.Oral];
            var cpExposureFractions = FakeConsumerProductExposureFractionsGenerator.Create(substances, products, config.SelectedExposureRoutes);

            var individualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue>();
            var individualsGenerator = new IndividualsGenerator();
            var simIndividuals = individualsGenerator
                .GenerateSimulatedIndividuals(
                    new Population() { PopulationIndividualPropertyValues = individualPropertyValues },
                    numberOfIndividuals,
                    1,
                    random
                );
            var individuals = simIndividuals.Select(c => c.Individual).ToList();
            var sexes = individuals.Select(c => c.Gender).Distinct().ToList();
            var ages = individuals.Select(c => c.Age).Distinct().ToList();
            var cpApplicationAmounts = FakeConsumerProductApplicationAmountsGenerator.Create(products, sexes, ages);
            var cpUseFrequencies = FakeConsumerProductUseFrequenciesGenerator.Create(individuals, products, 6);
            var concentrationModelsBuilder = new ConsumerProductConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                cpConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                AllConsumerProducts = products,
                SelectedPopulation = selectedPopulation,
                AllConsumerProductConcentrations = cpConcentrations,
                ConsumerProductExposureFractions = cpExposureFractions,
                ConsumerProductApplicationAmounts = cpApplicationAmounts,
                AllIndividualConsumerProductUseFrequencies = cpUseFrequencies,
                ConsumerProductConcentrationUnit = cpConcentrations.FirstOrDefault().Unit,
                ConsumerProductConcentrationModels = concentrationModels,
            };

            var calculator = new ConsumerProductExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestConsumerProductExposures");
            _ = calculator.Run(data, new CompositeProgressState());

            Assert.IsNotNull(data.ConsumerProductIndividualExposures);
            Assert.IsNotNull(data.ConsumerProductExposureUnit);
            Assert.AreEqual(numberOfIndividuals, data.ConsumerProductIndividualExposures.Count);
        }
    }
}

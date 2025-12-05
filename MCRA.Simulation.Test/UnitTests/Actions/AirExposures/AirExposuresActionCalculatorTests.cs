using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.AirExposures;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions.AirExposures {
    /// <summary>
    /// Runs the AirExposures action
    /// </summary>
    [TestClass]
    public class AirExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the AirExposures action: simulate individuals
        /// </summary>
        [TestMethod]
        public void AirExposuresActionCalculator_TestSimulate() {
            var seed = 1;
            var numberOfIndividuals = 10;
            var random = new McraRandomGenerator(seed);

            var substances = FakeSubstancesGenerator.Create(1);
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

            var indoorAirConcentrations = FakeAirConcentrationDistributionsGenerator.CreateIndoor(substances, seed);
            var outdoorAirConcentrations = FakeAirConcentrationDistributionsGenerator.CreateOutdoor(substances, seed);


            var project = new ProjectDto();
            var config = project.AirExposuresSettings;
            config.SelectedExposureRoutes = [ExposureRoute.Inhalation, ExposureRoute.Oral];
            config.AirExposuresIndividualGenerationMethod = AirExposuresIndividualGenerationMethod.Simulate;
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
            var airIndoorFractions = FakeAirIndoorFractionsGenerator.Create(ages, seed);
            var airVentilatoryFlowRates = FakeAirVentilatoryFlowRatesGenerator.Create(sexes, ages, seed);

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                SelectedPopulation = selectedPopulation,
                IndoorAirConcentrations = indoorAirConcentrations,
                OutdoorAirConcentrations = outdoorAirConcentrations,
                AirVentilatoryFlowRates = airVentilatoryFlowRates,
                AirIndoorFractions = airIndoorFractions,
                IndoorAirConcentrationUnit = indoorAirConcentrations.FirstOrDefault().Unit,
                Individuals = IndividualDaysGenerator.CreateSimulatedIndividualDays(simIndividuals),
            };

            var calculator = new AirExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestAirExposures");
            _ = calculator.Run(data, new CompositeProgressState());

            Assert.IsNotNull(data.IndividualAirExposures);
            Assert.IsNotNull(data.AirExposureUnit);
            Assert.HasCount(numberOfIndividuals, data.IndividualAirExposures);
        }

        /// <summary>
        /// Runs the AirExposures action: use individuals from dietary exposures
        /// </summary>
        [TestMethod]
        public void AirExposuresActionCalculator_TestDietary() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numberOfIndividuals = 10;

            var substances = FakeSubstancesGenerator.Create(1);

            var properties = FakeIndividualPropertiesGenerator.Create();
            properties.Add(FakeIndividualPropertiesGenerator.CreateFake(
                "BSA",
                IndividualPropertyType.Nonnegative,
                min: 1.8,
                max: 3.5
                )
            );
            var individuals = FakeIndividualsGenerator.Create(numberOfIndividuals, 1, false, properties, random);
            var foodsAsMeasured = FakeFoodsGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var dietaryIndividualDayIntakes = FakeDietaryIndividualDayIntakeGenerator.Create(individualDays, foodsAsMeasured, substances, 0, true, random);

            var indoorAirConcentrations = FakeAirConcentrationDistributionsGenerator.CreateIndoor(substances, seed);
            var outdoorAirConcentrations = FakeAirConcentrationDistributionsGenerator.CreateOutdoor(substances, seed);
            var sexes = individuals.Select(c => c.Gender).Distinct().ToList();
            var ages = individuals.Select(c => c.Age).Distinct().ToList();
            var airIndoorFractions = FakeAirIndoorFractionsGenerator.Create(ages, seed);
            var airVentilatoryFlowRates = FakeAirVentilatoryFlowRatesGenerator.Create(sexes, ages, seed);

            var project = new ProjectDto();
            var config = project.AirExposuresSettings;
            config.SelectedExposureRoutes = [ExposureRoute.Dermal, ExposureRoute.Oral];
            config.AirExposuresIndividualGenerationMethod = AirExposuresIndividualGenerationMethod.UseDietaryExposures;

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                IndoorAirConcentrations = indoorAirConcentrations,
                OutdoorAirConcentrations = outdoorAirConcentrations,
                AirIndoorFractions = airIndoorFractions,
                AirVentilatoryFlowRates = airVentilatoryFlowRates,
                IndoorAirConcentrationUnit = indoorAirConcentrations.FirstOrDefault().Unit,
            };
                
            var calculator = new AirExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestAirExposures");
            _ = calculator.Run(data, new CompositeProgressState());

            Assert.IsNotNull(data.IndividualAirExposures);
            Assert.IsNotNull(data.AirExposureUnit);
            Assert.HasCount(numberOfIndividuals, data.IndividualAirExposures);
        }
    }
}

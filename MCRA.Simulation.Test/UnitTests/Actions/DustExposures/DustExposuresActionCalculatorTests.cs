using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.DustExposures;
using MCRA.Simulation.Calculators.ConcentrationModelBuilder;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions.DustExposures {
    /// <summary>
    /// Runs the DustExposures action
    /// </summary>
    [TestClass]
    public class ExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the DustExposures action: simulate individuals
        /// </summary>
        [TestMethod]
        public void DustExposuresActionCalculator_TestSimulate() {
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
            var dustConcentrations = FakeDustConcentrationsGenerator.Create(substances, seed);
            var dustIngestions = FakeDustIngestionsGenerator.Create(seed);
            var dustAdherenceAmounts = FakeDustAdherenceAmountsGenerator.Create(seed);
            var dustAvailabilityFractions = FakeDustAvailabilityFractionsGenerator.Create(substances, seed);
            var dustConcentrationModelBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = dustConcentrationModelBuilder.Create(
                dustConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0);

            var project = new ProjectDto();
            var config = project.DustExposuresSettings;
            config.SelectedExposureRoutes = [ExposureRoute.Dermal, ExposureRoute.Oral];
            config.DustExposuresIndividualGenerationMethod = DustExposuresIndividualGenerationMethod.Simulate;
            var individualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue>();
            var individualsGenerator = new IndividualsGenerator();
            var individuals = individualsGenerator
                .GenerateSimulatedIndividuals(
                    new Population() { PopulationIndividualPropertyValues = individualPropertyValues },
                    numberOfIndividuals,
                    1,
                    random
                );
            var sexes = individuals.Select(c => c.Gender).Distinct().ToList();
            var ages = individuals.Select(c => c.Age).Distinct().ToList();
            var dustBodyExposureFractions = FakeDustBodyExposureFractionsGenerator.Create(sexes, ages, seed);

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                SelectedPopulation = selectedPopulation,
                DustConcentrationModels = concentrationModels,  
                DustIngestions = dustIngestions,
                DustBodyExposureFractions = dustBodyExposureFractions,
                DustAdherenceAmounts = dustAdherenceAmounts,
                DustAvailabilityFractions = dustAvailabilityFractions,
                DustConcentrationDistributionUnit = dustConcentrations.FirstOrDefault().Unit,
                DustIngestionUnit = dustIngestions.FirstOrDefault().ExposureUnit,
                Individuals = IndividualDaysGenerator.CreateSimulatedIndividualDays(individuals),
            };

            var calculator = new DustExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestDustExposures");
            var result = calculator.Run(data, new CompositeProgressState());

            var numberOfSimulatedDustIndividualDayExposures = numberOfIndividuals;

            Assert.IsNotNull(data.IndividualDustExposures);
            Assert.IsNotNull(data.DustExposureUnit);
            Assert.HasCount(numberOfIndividuals, data.IndividualDustExposures);
        }

        /// <summary>
        /// Runs the DustExposures action: use individuals from dietary exposures
        /// </summary>
        [TestMethod]
        public void DustExposuresActionCalculator_TestDietary() {
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
            var sexes = individuals.Select(c => c.Gender).Distinct().ToList();
            var ages = individuals.Select(c => c.Age).Distinct().ToList();
            var dustConcentrations = FakeDustConcentrationsGenerator.Create(substances, seed);
            var dustIngestions = FakeDustIngestionsGenerator.Create(seed);
            var dustBodyExposureFractions = FakeDustBodyExposureFractionsGenerator.Create(sexes, ages, seed);
            var dustAdherenceAmounts = FakeDustAdherenceAmountsGenerator.Create(seed);
            var dustAvailabilityFractions = FakeDustAvailabilityFractionsGenerator.Create(substances, seed);

            var dustConcentrationModelBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = dustConcentrationModelBuilder.Create(
                dustConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0);

            var project = new ProjectDto();
            var config = project.DustExposuresSettings;
            config.SelectedExposureRoutes = [ExposureRoute.Dermal, ExposureRoute.Oral];
            config.DustExposuresIndividualGenerationMethod = DustExposuresIndividualGenerationMethod.UseDietaryExposures;

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                DustConcentrationModels = concentrationModels,
                DustIngestions = dustIngestions,
                DustBodyExposureFractions = dustBodyExposureFractions,
                DustAdherenceAmounts = dustAdherenceAmounts,
                DustAvailabilityFractions = dustAvailabilityFractions,
                DustConcentrationDistributionUnit = dustConcentrations.FirstOrDefault().Unit,
                DustIngestionUnit = dustIngestions.FirstOrDefault().ExposureUnit
            };

            var calculator = new DustExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestDustExposures");
            var result = calculator.Run(data, new CompositeProgressState());

            Assert.IsNotNull(data.IndividualDustExposures);
            Assert.IsNotNull(data.DustExposureUnit);
            Assert.HasCount(numberOfIndividuals, data.IndividualDustExposures);
        }
    }
}

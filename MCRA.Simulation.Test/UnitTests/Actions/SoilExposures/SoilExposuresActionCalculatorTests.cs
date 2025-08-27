﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.SoilExposures;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Actions.SoilExposures {
    /// <summary>
    /// Runs the SoilExposures action
    /// </summary>
    [TestClass]
    public class ExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the SoilExposures action: simulate individuals
        /// </summary>
        [TestMethod]
        public void SoilExposuresActionCalculator_TestSimulate() {
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

            var soilConcentrations = FakeSoilConcentrationDistributionsGenerator.Create(substances, seed);

            var project = new ProjectDto();
            var config = project.SoilExposuresSettings;
            config.SoilExposuresIndividualGenerationMethod = SoilExposuresIndividualGenerationMethod.Simulate;
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
            var soilIngestions = FakeSoilIngestionsGenerator.Create(sexes, ages, seed);

            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                SelectedPopulation = selectedPopulation,
                SoilConcentrationDistributions = soilConcentrations,
                SoilIngestions = soilIngestions,
                SoilConcentrationUnit = soilConcentrations.FirstOrDefault().Unit,
                SoilIngestionUnit = soilIngestions.FirstOrDefault().ExposureUnit,
                Individuals = IndividualDaysGenerator.CreateSimulatedIndividualDays(individuals),
            };

            var calculator = new SoilExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestSoilExposures1");
            _ = calculator.Run(data, new CompositeProgressState());

            Assert.IsNotNull(data.IndividualSoilExposures);
            Assert.IsNotNull(data.SoilExposureUnit);
            Assert.AreEqual(numberOfIndividuals, data.IndividualSoilExposures.Count);
        }

        /// <summary>
        /// Runs the SoilExposures action: use individuals from dietary exposures
        /// </summary>
        [TestMethod]
        public void SoilExposuresActionCalculator_TestDietary() {
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
            var soilConcentrations = FakeSoilConcentrationDistributionsGenerator.Create(substances, seed);

            var project = new ProjectDto();
            var config = project.SoilExposuresSettings;
            config.SoilExposuresIndividualGenerationMethod = SoilExposuresIndividualGenerationMethod.UseDietaryExposures;
            var sexes = individuals.Select(c => c.Gender).Distinct().ToList();
            var ages = individuals.Select(c => c.Age).Distinct().ToList();
            var soilIngestions = FakeSoilIngestionsGenerator.Create(sexes, ages, seed);
            var data = new ActionData() {
                AllCompounds = substances,
                ActiveSubstances = substances,
                DietaryIndividualDayIntakes = dietaryIndividualDayIntakes,
                SoilConcentrationDistributions = soilConcentrations,
                SoilIngestions = soilIngestions,
                SoilConcentrationUnit = soilConcentrations.FirstOrDefault().Unit,
                SoilIngestionUnit = soilIngestions.FirstOrDefault().ExposureUnit
            };

            var calculator = new SoilExposuresActionCalculator(project);
            TestRunUpdateSummarizeNominal(project, calculator, data, "TestSoilExposures2");
            _ = calculator.Run(data, new CompositeProgressState());

            Assert.IsNotNull(data.IndividualSoilExposures);
            Assert.IsNotNull(data.SoilExposureUnit);
            Assert.AreEqual(numberOfIndividuals, data.IndividualSoilExposures.Count);
        }
    }
}

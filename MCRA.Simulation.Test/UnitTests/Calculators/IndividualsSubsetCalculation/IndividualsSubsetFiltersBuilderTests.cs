using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.Filters.IndividualFilters;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IndividualsSubsetCalculation {

    /// <summary>
    /// Individuals subset filters builder tests.
    /// </summary>
    [TestClass]
    public class IndividualsSubsetFiltersBuilderTests {

        /// <summary>
        /// Test construction of an individual subset filters collection from a
        /// population specified by age and gender.
        /// </summary>
        [TestMethod]
        public void IndividualsSubsetFiltersBuilder_TestCreateFromPopulation() {
            var ageProperty = MockIndividualPropertiesGenerator.FakeAgeProperty;
            var populationAgeRange = new PopulationIndividualPropertyValue() {
                IndividualProperty = ageProperty,
                MinValue = 1,
                MaxValue = 9
            };

            var genderProperty = MockIndividualPropertiesGenerator.FakeGenderProperty;
            var populationGenders = new PopulationIndividualPropertyValue() {
                IndividualProperty = genderProperty,
                Value = "Male"
            };

            var categoricalProperty = MockIndividualPropertiesGenerator
                .CreateFake("Categorical", IndividualPropertyType.Categorical);
            var populationCategoricalPropertyValues = new PopulationIndividualPropertyValue() {
                IndividualProperty = categoricalProperty,
                Value = "Black,Red"
            };

            var booleanProperty = MockIndividualPropertiesGenerator.FakeBooleanProperty;
            var booleanPropertyValue = new PopulationIndividualPropertyValue() {
                IndividualProperty = booleanProperty,
                Value = "T"
            };

            var population = MockPopulationsGenerator.Create(1).First();
            population.PopulationIndividualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue>() {
                { ageProperty.Code, populationAgeRange },
                { genderProperty.Code, populationGenders },
                { categoricalProperty.Code, populationCategoricalPropertyValues },
                { booleanProperty.Code, booleanPropertyValue }
            };

            var surveyIndividualProperties = new List<IndividualProperty>() {
                ageProperty,
                genderProperty,
                categoricalProperty,
                booleanProperty
            }.ToDictionary(c =>c.Code, c=> c);
            var subsetDefinitions = new List<string>();

            var builder = new IndividualsSubsetFiltersBuilder();
            var filters = builder.Create(
                population, 
                surveyIndividualProperties, 
                IndividualSubsetType.MatchToPopulationDefinition,
                subsetDefinitions
            );
            Assert.AreEqual(4, filters.Count);

            var ageFilter = filters.FirstOrDefault(r => r.IndividualProperty == ageProperty);
            Assert.IsNotNull(ageFilter);
            Assert.IsTrue(ageFilter is NumericPropertyIndividualFilter);
            Assert.AreEqual(1, (ageFilter as NumericPropertyIndividualFilter).Min.Value);
            Assert.AreEqual(9, (ageFilter as NumericPropertyIndividualFilter).Max.Value);

            var genderFilter = filters.FirstOrDefault(r => r.IndividualProperty == genderProperty);
            Assert.IsNotNull(genderFilter);
            Assert.IsTrue(genderFilter is GenderPropertyIndividualFilter);

            var categoricalFilter = filters.FirstOrDefault(r => r.IndividualProperty == categoricalProperty);
            Assert.IsNotNull(categoricalFilter);
            Assert.IsTrue(categoricalFilter is CategoricalPropertyIndividualFilter);
            CollectionAssert.AreEquivalent(
                new[] { "Black", "Red" },
                (categoricalFilter as CategoricalPropertyIndividualFilter).AcceptedValues.ToArray()
            );

            var booleanPropertyFilter = filters.FirstOrDefault(r => r.IndividualProperty == booleanProperty);
            Assert.IsNotNull(booleanPropertyFilter);
            Assert.IsTrue(booleanPropertyFilter is BooleanPropertyIndividualFilter);
        }

        /// <summary>
        /// Test creation of filters for population individual properties missing in the
        /// survey individuals.
        /// </summary>
        [TestMethod]
        public void IndividualsSubsetFiltersBuilder_TestCreateFromPopulation_Missing() {
            var genderProperty = MockIndividualPropertiesGenerator.FakeGenderProperty;
            var populationGenders = new PopulationIndividualPropertyValue() {
                IndividualProperty = genderProperty,
                Value = "Male"
            };

            var population = MockPopulationsGenerator.Create(1).First();
            population.PopulationIndividualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue>() {
                { genderProperty.Code, populationGenders }
            };

            // Empty list
            var surveyIndividualProperties = new Dictionary<string, IndividualProperty>();
            var subsetDefinitions = new List<string>();
            var builder = new IndividualsSubsetFiltersBuilder();
            var filters = builder.Create(
                population, 
                surveyIndividualProperties,
                IndividualSubsetType.MatchToPopulationDefinition, 
                subsetDefinitions
            );

            // Should pass, no filters should be added
            Assert.AreEqual(0, filters.Count);
        }
    }
}

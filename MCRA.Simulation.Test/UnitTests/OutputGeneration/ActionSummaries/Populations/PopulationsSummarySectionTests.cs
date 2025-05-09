﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Populations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Populations
    /// </summary>
    [TestClass]
    public class PopulationsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test PopulationsSummarySection view
        /// </summary>
        [TestMethod]
        public void PopulationsSummarySection_Test1() {
            var section = new PopulationPropertySummarySection();

            var propertyValueDict = new Dictionary<string, PopulationIndividualPropertyValue>();
            var propertyValue = new PopulationIndividualPropertyValue() {
                IndividualProperty = FakeIndividualPropertiesGenerator.FakeAgeProperty,
                MinValue = 0,
                MaxValue = 10,
            };
            propertyValueDict["Age"] = propertyValue;

            var population = new Population() {
                Code = "Population",
                Name = "Population",
                Description = "Population",
                Location = "Nl",
                NominalBodyWeight = 70,
                PopulationIndividualPropertyValues = propertyValueDict
            };
            section.Summarize(population);
            RenderView(section, filename: "TestSummarize.html");
            Assert.IsNotNull(section.Records);
            AssertIsValidView(section);
        }
    }
}
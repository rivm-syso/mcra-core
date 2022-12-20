using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringData {
    /// <summary>
    /// OutputGeneration, ActionSummaries, HumanMonitoringData
    /// </summary>
    [TestClass]
    public class HumanMonitoringSurveySummarySectionSectionTests : SectionTestBase {

        /// <summary>
        /// Test summarize.
        /// </summary>
        [TestMethod]
        public void HumanMonitoringSurveySummarySectionSection_TestSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var surveys = MockHumanMonitoringDataGenerator.MockHumanMonitoringSurveys(individualDays);
            var section = new HbmSurveySummarySection();

            var selectedPopulation = MockPopulationsGenerator.Create(1).First();
            var populationIndividualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue>();
            populationIndividualPropertyValues["Month"] = new PopulationIndividualPropertyValue() {
                Value = "1,2,3,4,5,6,7,8,9,10"
            };
            populationIndividualPropertyValues["Region"] = new PopulationIndividualPropertyValue() {
                Value = "Location1"
            };
            selectedPopulation.StartDate = new DateTime(2022, 1, 1);
            selectedPopulation.EndDate = new DateTime(2022, 12, 31);
            selectedPopulation.PopulationIndividualPropertyValues = populationIndividualPropertyValues;
            selectedPopulation.Location = "Location1";

            section.Summarize(
                surveys, 
                individuals, 
                selectedPopulation, 
                IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties,
                populationIndividualPropertyValues.Keys.ToList()
            );
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test view.
        /// </summary>
        [TestMethod]
        public void HumanMonitoringSurveySummarySectionSection_Test1() {
            var section = new HbmSurveySummarySection();
            section.Records = new List<HbmSurveySummaryRecord>() {
                new HbmSurveySummaryRecord() {
                    SurveyProperties = new List<HbmSurveyPropertyRecord>() {
                        new HbmSurveyPropertyRecord() {
                            Code= "survey",
                            CovariateName ="age",
                            Description= "descr",
                            Level = "1",
                            Maximum=99,
                            Minimum =1,
                            PropertyType ="type",
                        }
                    }
                }
            };
            AssertIsValidView(section);
        }
    }
}

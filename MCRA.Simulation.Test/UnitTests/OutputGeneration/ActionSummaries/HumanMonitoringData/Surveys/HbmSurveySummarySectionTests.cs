using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData.Individuals;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var survey = FakeHbmDataGenerator.FakeHbmSurvey(individualDays);
            var section = new HbmSurveySummarySection();

            var selectedPopulation = FakePopulationsGenerator.Create(1).First();
            var populationIndividualPropertyValues = new Dictionary<string, PopulationIndividualPropertyValue> {
                ["Month"] = new PopulationIndividualPropertyValue() {
                    Value = "1,2,3,4,5,6,7,8,9,10"
                },
                ["Region"] = new PopulationIndividualPropertyValue() {
                    Value = "Location1"
                }
            };
            selectedPopulation.StartDate = new DateTime(2022, 1, 1);
            selectedPopulation.EndDate = new DateTime(2022, 12, 31);
            selectedPopulation.PopulationIndividualPropertyValues = populationIndividualPropertyValues;
            selectedPopulation.Location = "Location1";

            section.Summarize(
                survey,
                selectedPopulation
            );
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test view.
        /// </summary>
        [TestMethod]
        public void HumanMonitoringSurveySummarySectionSection_Test1() {
            var section = new HbmSurveySummarySection {
                Record = new HbmSurveySummaryRecord() {
                    SurveyProperties = [
                        new HbmSurveyPropertyRecord() {
                            Code= "survey",
                            CovariateName ="age",
                            Description= "descr",
                            Level = "1",
                            Maximum=99,
                            Minimum =1,
                            PropertyType ="type",
                        }
                    ]
                }
            };
            AssertIsValidView(section);
        }
    }
}

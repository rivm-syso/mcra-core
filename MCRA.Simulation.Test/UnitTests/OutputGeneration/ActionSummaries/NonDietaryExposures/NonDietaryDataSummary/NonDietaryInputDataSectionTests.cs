using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.NonDietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, NonDietaryExposures, NonDietaryDataSummary
    /// </summary>
    [TestClass]
    public class NonDietaryInputDataSectionTests : SectionTestBase {

        /// <summary>
        /// Test NonDietaryInputDataSection view
        /// </summary>
        [TestMethod]
        public void NondietaryInputDataSection_Test1() {
            var section = new NonDietaryInputDataSection();
            section.NonDietaryInputDataRecords = [];
            section.NonDietarySurveyPropertyRecords = [
                new NonDietarySurveyPropertyRecord() {
                    Code= "survey",
                    CovariateName ="age",
                    Description= "descr",
                    Level = "1",
                    Maximum=99,
                    Minimum =1,
                    PropertyType ="type",
                }
            ];
            section.NonDietarySurveyProbabilityRecords = [];
            AssertIsValidView(section);
            section.NonDietaryInputDataRecords.Add(new NonDietaryInputDataRecord());
            AssertIsValidView(section);
        }
    }
}
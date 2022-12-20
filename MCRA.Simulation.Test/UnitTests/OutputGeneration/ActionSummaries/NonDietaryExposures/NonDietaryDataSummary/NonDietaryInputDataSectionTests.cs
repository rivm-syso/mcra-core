using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            section.NonDietaryInputDataRecords = new List<NonDietaryInputDataRecord>();
            section.NonDietarySurveyPropertyRecords = new List<NonDietarySurveyPropertyRecord>() {
                new NonDietarySurveyPropertyRecord() {
                    Code= "survey",
                    CovariateName ="age",
                    Description= "descr",
                    Level = "1",
                    Maximum=99,
                    Minimum =1,
                    PropertyType ="type",
                }
            };
            section.NonDietarySurveyProbabilityRecords = new List<NonDietaryExposureProbabilityRecord>();
            AssertIsValidView(section);
            section.NonDietaryInputDataRecords.Add(new NonDietaryInputDataRecord());
            AssertIsValidView(section);
        }
    }
}
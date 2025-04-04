﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DoseResponseModels
    /// </summary>
    [TestClass]
    public class DoseResponseModelSectionTests : SectionTestBase {

        /// <summary>
        /// Test DoseResponseModelSection view
        /// </summary>
        [TestMethod]
        public void DoseResponseModelSection_Test1() {
            var section = new DoseResponseModelSection();
            section.DoseResponseFits = [
                new DoseResponseFitRecord() {
                    SubstanceName ="C",
                    SubstanceCode = "C",
                    BenchmarkDose = 1.2,
                    BenchmarkDoseLower = 1,
                    BenchmarkDoseUpper = 1.5,
                    BenchmarkResponse = .05,
                    BenchmarkDosesUncertain= [1.02, 1.13],
                    RelativePotencyFactor= 1,
                    RpfLower = 0.9,
                    RpfUpper = 1.1,
                    RpfUncertain = [1.1, 1.2],
                    CovariateLevel ="",
                    ModelParameterValues=""
                }
            ];
            section.SubstanceNames = ["A", "B"];
            section.DoseResponseSets = [];
            AssertIsValidView(section);
        }
    }
}
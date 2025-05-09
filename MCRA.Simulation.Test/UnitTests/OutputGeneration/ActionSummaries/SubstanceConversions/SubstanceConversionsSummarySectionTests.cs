﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.SubstanceConversions {
    /// <summary>
    /// OutputGeneration, ActionSummaries, SubstanceConversions
    /// </summary>
    [TestClass]
    public class SubstanceConversionsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test SubstanceConversionsSummarySection view
        /// </summary>
        [TestMethod]
        public void SubstanceConversionsSummarySection_Test1() {
            var substances = FakeSubstancesGenerator.Create(5);
            var substanceConversions = new List<SubstanceConversion> {
                new() {
                    MeasuredSubstance = substances[0],
                    ActiveSubstance = substances[1],
                    ConversionFactor = .5,
                    Proportion = .5,
                    IsExclusive = true,
                }
            };
            var section = new SubstanceConversionsSummarySection();
            section.Summarize(substanceConversions, null);
            AssertIsValidView(section);
        }
    }
}
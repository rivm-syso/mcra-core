﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.QsarMembershipModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, QsarMembershipModels
    /// </summary>
    [TestClass]
    public class QsarMembershipModelCorrelationsSectionTests : SectionTestBase {

        /// <summary>
        /// Test QsarMembershipModelCorrelationsSection view
        /// </summary>
        [TestMethod]
        public void QsarMembershipModelCorrelationsSection_Test1() {
            var section = new QsarMembershipModelCorrelationsSection();
            section.ModelNames = ["A", "B"];
            section.PearsonCorrelations = [new List<double>() { 1, 2 }];
            AssertIsValidView(section);
        }
    }
}
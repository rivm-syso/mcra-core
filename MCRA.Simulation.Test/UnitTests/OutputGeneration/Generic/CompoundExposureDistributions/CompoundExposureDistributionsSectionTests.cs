﻿using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Generic {
    /// <summary>
    /// OutputGeneration, Generic, CompoundExposureDistributions
    /// </summary>
    [TestClass]
    public class CompoundExposureDistributionsSectionTests : SectionTestBase {
        /// <summary>
        /// Test CompoundExposureDistributionsSection view
        /// </summary>
        [TestMethod]
        public void CompoundExposureDistributionSection_Test1() {
            var section = new CompoundExposureDistributionsSection() {
                CompoundExposureDistributionRecords = [
                    new CompoundExposureDistributionRecord(){
                        HistogramBins = [],
                    }
                ],
            };
            AssertIsValidView(section);
        }
    }
}
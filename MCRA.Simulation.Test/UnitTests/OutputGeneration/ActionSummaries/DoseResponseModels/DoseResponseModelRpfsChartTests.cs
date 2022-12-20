using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DoseResponseModels
    /// </summary>
    [TestClass]
    public class DoseResponseModelsChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void DoseResponseModelRpfsChart_Test1() {

            var result = new List<DoseResponseFitRecord>();
            result.Add(new DoseResponseFitRecord() {
                RpfLower = .5,
                RpfUpper = 1.5,
                RelativePotencyFactor = 1,
                SubstanceName = "Sub1",
                SubstanceCode = "Sub1"
            });
            var section = new DoseResponseModelSection() {
                DoseResponseFits = result
            };

            var chart = new DoseResponseModelRpfsChartCreator(section, false);
            RenderChart(chart, "TestCreate");
        }
    }
}

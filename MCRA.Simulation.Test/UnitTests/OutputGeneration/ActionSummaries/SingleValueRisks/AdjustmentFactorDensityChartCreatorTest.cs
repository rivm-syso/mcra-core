using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.SingleValueRisks {

    /// <summary>
    /// OutputGeneration, ActionSummaries, SingleValueRisks
    /// </summary>
    [TestClass]
    public class AdjustmentFactorDensityChartCreatorTest : ChartCreatorTestBase {

        /// <summary>
        /// doi: 10.2903/j.efsa.2020.6087
        /// Cumulative dietary risk characterisation of pesticides that have acute effects on the nervous system
        /// p27 and p30
        /// Test SingleValueRisk view
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorDensityChartCreator_Test1() {
            var section = new SingleValueRisksAdjustmentFactorsSection() {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta,
                ExposureParameterA = 2.87,
                ExposureParameterB = 4.26,
                ExposureParameterC = 0.5,
                ExposureParameterD = 6,
                HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta,
                HazardParameterA = 1.89,
                HazardParameterB = 3.51,
                HazardParameterC = 0.75,
                HazardParameterD = 4,
            };
            var chart = new AFDensityChartCreator(section, true);
            RenderChart(chart, $"TestCreateEFSAAcuteBetaTab7");
            var chart1 = new AFDensityChartCreator(section, false);
            RenderChart(chart1, $"TestCreateEFSAAcuteBetaTab9");
        }

        /// <summary>
        /// doi: 10.2903/j.efsa.2020.6087
        /// Cumulative dietary risk characterisation of pesticides that have acute effects on the nervous system
        /// p25 and p31
        /// Test SingleValueRisk view
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorDensityChartCreator_Test2() {
            var section = new SingleValueRisksAdjustmentFactorsSection() {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Gamma,
                ExposureParameterA = 3.26,
                ExposureParameterB = 3.56,
                ExposureParameterC = 0.9,
                HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Gamma,
                HazardParameterA = 2.42,
                HazardParameterB = 1.1,
                HazardParameterC = 0.9,
            };
            var chart = new AFDensityChartCreator(section, true);
            RenderChart(chart, $"TestCreateEFSAAcuteGammaTab6");
            var chart1 = new AFDensityChartCreator(section, false);
            RenderChart(chart1, $"TestCreateEFSAAcuteGammaTab10");
        }

        /// <summary>
        /// Test ResponseSummarySection view
        /// doi: 10.2903/j.efsa.2020.6088
        /// Cumulative dietary risk characterisation of pesticides that have chronic effects on the thyroid
        /// p24 and p25
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorDensityChartCreator_Test3() {
            var section = new SingleValueRisksAdjustmentFactorsSection() {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Gamma,
                ExposureParameterA = 2.13,
                ExposureParameterB = 2.24,
                ExposureParameterC = 0.3,
                HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Gamma,
                HazardParameterA = 3.84,
                HazardParameterB = 3.28,
                HazardParameterC = 0.5,
            };
            var chart = new AFDensityChartCreator(section, true);
            RenderChart(chart, $"TestCreateEFSAChronicGammaTab6");
            var chart1 = new AFDensityChartCreator(section, false);
            RenderChart(chart1, $"TestCreateEFSAChronicGammaTab5");
        }

        /// <summary>
        /// Test ResponseSummarySection view
        /// doi: 10.2903/j.efsa.2020.6088
        /// Cumulative dietary risk characterisation of pesticides that have chronic effects on the thyroid
        /// p28 and p29
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorDensityChartCreator_Test4() {
            var section = new SingleValueRisksAdjustmentFactorsSection() {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.LogNormal,
                ExposureParameterA = 0.705,
                ExposureParameterB = 0.566,
                ExposureParameterC = 1,
                HazardAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.LogStudents_t,
                HazardParameterA = -0.593,
                HazardParameterB = 0.367,
                HazardParameterC = 3,
                HazardParameterD = 0.5,
            };
            var chart = new AFDensityChartCreator(section, true);
            RenderChart(chart, $"TestCreateEFSAChronicLogNormalTab8");
            var chart1 = new AFDensityChartCreator(section, false);
            RenderChart(chart1, $"TestCreateEFSAChronicLogStudentTab9");
        }
    }
}

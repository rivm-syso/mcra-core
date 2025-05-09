﻿using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Calculators.ComponentCalculation.HClustCalculation;
using MCRA.Utils;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ExposureMixtures
    /// </summary>
    [TestClass]
    public class HClustChartTests : ChartCreatorTestBase {

        #region Fakes

        private static IndividualMatrix fakeExposuresMatrix() {
            var random = new McraRandomGenerator(1);
            var substances = FakeSubstancesGenerator.Create(10);
            var substanceTargets = substances
                .Select(r => (r, ExposureTarget.DefaultInternalExposureTarget))
                .ToList();
            var individuals = FakeIndividualsGenerator.Create(50, 2, random);
            var individualMatrix = FakeExposureMatrixGenerator.CreateIndividualMatrix(
                individuals.Select(r => r.Id).ToList(),
                substanceTargets,
                numberOfComponents: 4,
                numberOfZeroExposureRecords: 0,
                numberOfZeroExposureSubstances: 0,
                sigma: 1,
                seed: random.Next()
            );
            return individualMatrix;
        }

        #endregion
        /// <summary>
        /// Create charts and test Kmeans view
        /// </summary>
        [TestMethod]
        public void HClustChart_Test1() {
            var individualComponentMatrix = fakeExposuresMatrix();

            var calculator = new HClustCalculator(3, false);
            var clusterResult = calculator.Compute(individualComponentMatrix, new GeneralMatrix(1, individualComponentMatrix.VMatrix.RowDimension, 1));
            individualComponentMatrix.ClusterResult = clusterResult;
            var section = new HClustSection();
            var normalizationFactorU = Enumerable.Repeat(1d, individualComponentMatrix.VMatrix.RowDimension).ToArray();
            section.Summarize(individualComponentMatrix, normalizationFactorU, false);

            var chart = new HClustChartCreator(section);
            RenderChart(chart, $"HClust1");
            AssertIsValidView(section);
        }
    }
}



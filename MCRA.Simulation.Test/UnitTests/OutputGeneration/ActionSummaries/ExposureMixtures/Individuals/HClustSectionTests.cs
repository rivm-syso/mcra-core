﻿using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils;
using MCRA.Simulation.Calculators.ComponentCalculation.HClustCalculation;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ExposureMixtures.HClust {
    [TestClass]
    public class HClustSectionTests : SectionTestBase {

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
        /// Test summarize section based on fake exposures matrix.
        /// </summary>
        [TestMethod]
        public void HClustSection_TestSummarize() {
            var individualMatrix = fakeExposuresMatrix();
            var clusterResult1 = new ClusterRecord() {
                ClusterId = 1,
                SimulatedIndividuals = individualMatrix.SimulatedIndividuals.Take(10).ToList(),
                Indices = individualMatrix.SimulatedIndividuals.Take(10).Select(c => c.Id).ToList()
            };
            var clusterResult2 = new ClusterRecord() {
                ClusterId = 2,
                SimulatedIndividuals = individualMatrix.SimulatedIndividuals.Skip(10).ToList(),
                Indices = individualMatrix.SimulatedIndividuals.Skip(10).Select(c => c.Id).ToList()
            };
            individualMatrix.ClusterResult = new ClusterResult() {
                Clusters = [clusterResult1, clusterResult2]
            };
            var normalizationFactorU = Enumerable.Repeat(1d, individualMatrix.VMatrix.RowDimension).ToArray();
            var section = new HClustSection();
            section.Summarize(individualMatrix, normalizationFactorU, false);
        }

        /// <summary>
        /// Test renderign of the section view.
        /// </summary>
        [TestMethod]
        public void HClustSection_TestValidView() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualComponentMatrix = fakeExposuresMatrix();
            var calculator = new HClustCalculator(3, false);
            var clusterResult = calculator.Compute(individualComponentMatrix, new GeneralMatrix(1, individualComponentMatrix.VMatrix.RowDimension, 1));

            var section = new HClustSection() {
                Clusters = clusterResult.Clusters.Select(c => c.SimulatedIndividuals.Count).ToList(),
                IndividualCodes = individualComponentMatrix.SimulatedIndividuals.Select(c => c.Code).ToList(),
                ComponentCodes = Enumerable.Range(1, individualComponentMatrix.NumberOfComponents).Select(c => c.ToString()).ToList(),
                VMatrix = individualComponentMatrix.VMatrix,
                ClusterResult = clusterResult
            };
            AssertIsValidView(section);
            RenderView(section, filename: "TestValidView.html");
        }
    }
}

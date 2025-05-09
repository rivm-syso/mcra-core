﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.UnitVariabilityCalculation {
    /// <summary>
    /// UnitVariabilityCalculation calculator
    /// </summary>
    [TestClass]
    public class BetaDistributionModelTests {

        /// <summary>
        /// Test Beta distribution model. Generates a model using vf = 5 and
        /// 24 units in composite sample, draws a number of times from this model
        /// and computes the variability factor from these draws. Should be
        /// approximately equal to the initial vf.
        /// </summary>
        [TestMethod]
        public void BetaDistributionModelTest1() {
            var vf = 4;
            var unitsCompositeSample = 5;
            var food = new Food() {
                Name = "Apple",
            };
            var foodProperty = new FoodProperty() {
                UnitWeight = 100,
            };
            food.Properties = foodProperty;
            var uvFactor = new UnitVariabilityFactor() {
                Factor = vf,
                UnitsInCompositeSample = unitsCompositeSample,
            };

            // Create unit variability model
            var uvCalculator = new BetaDistributionModel(food, uvFactor, EstimatesNature.Realistic, UnitVariabilityType.VariabilityFactor);
            uvCalculator.CalculateParameters();

            // Draw n times
            var random = new McraRandomGenerator();
            var values = new List<IntakePortion>();
            var intakePortion = new IntakePortion() {
                Amount = 100,
                Concentration = 100,
            };
            for (int i = 0; i < 10000; ++i) {
                values.AddRange(uvCalculator.DrawFromDistribution(food, intakePortion, random));
            }
            var concentrations = values.Select(r => (double)r.Concentration).ToList();

            // Derive variability factor from drawn concentrations
            var mean = concentrations.Average();
            var p97p5 = concentrations.Percentile(97.5);
            var vf_computed = p97p5 / mean;
            Assert.AreEqual(vf, vf_computed, 2e-1);

            // Another way of computing the same
            var svf = concentrations.Select(c => c / mean).ToList();
            var svfp97p5 = concentrations.Select(c => c / mean).ToList().Percentile(97.5);
        }

        /// <summary>
        /// Test Beta distribution model. Generates a model using vf = 5 and
        /// 24 units in composite sample, draws a number of times from this model
        /// and computes the variability factor from these draws. Should be
        /// approximately equal to the initial vf.
        /// </summary>
        [TestMethod]
        public void BetaDistributionModelTest2() {
            var vf = 4.99;
            var unitsCompositeSample = 5;
            var food = new Food() {
                Name = "Apple",
            };
            var foodProperty = new FoodProperty() {
                UnitWeight = 100,
            };
            food.Properties = foodProperty;
            var uvFactor = new UnitVariabilityFactor() {
                Factor = vf,
                UnitsInCompositeSample = unitsCompositeSample,
            };

            // Create unit variability model
            var uvCalculator = new BetaDistributionModel(food, uvFactor, EstimatesNature.Realistic, UnitVariabilityType.VariabilityFactor);
            uvCalculator.CalculateParameters();

            // Draw n times
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var values = new List<IntakePortion>();
            var intakePortion = new IntakePortion() {
                Amount = 100,
                Concentration = 100,
            };
            for (int i = 0; i < 10000; ++i) {
                values.AddRange(uvCalculator.DrawFromDistribution(food, intakePortion, random));
            }
            var concentrations = values.Select(r => (double)r.Concentration).ToList();

            // Derive variability factor from drawn concentrations
            var mean = concentrations.Average();
            var p97p5 = concentrations.Percentile(97.5);
            var vf_computed = p97p5 / mean;
            Assert.AreEqual(vf, vf_computed, 2e-1);

            // Another way of computing the same
            var svf = concentrations.Select(c => c / mean).ToList();
            var svfp97p5 = concentrations.Select(c => c / mean).ToList().Percentile(97.5);
        }
    }
}

﻿using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    [TestClass]
    public class HbmIndividualDayConcentrationsCalculatorTests {
        internal class MockSettings : IHbmIndividualDayConcentrationsCalculatorSettings {
            public NonDetectsHandlingMethod NonDetectsHandlingMethod => NonDetectsHandlingMethod.ReplaceByLOR;
            public double LorReplacementFactor => 1;
            public MissingValueImputationMethod MissingValueImputationMethod => MissingValueImputationMethod.SetZero;
            public NonDetectImputationMethod NonDetectImputationMethod => NonDetectImputationMethod.CensoredLogNormal;
            public bool ImputeHbmConcentrationsFromOtherMatrices => false;
            public double HbmBetweenMatrixConversionFactor => 1;
        }

        [TestMethod]
        public void HbmIndividualDayConcentrationsCalculator_Test() {
            var settings = new MockSettings();
            var matrixConversionCalculator = new SimpleBiologicalMatrixConcentrationConversionCalculator(1);
            var calculator = new HbmIndividualDayConcentrationsCalculator(
                settings,
                matrixConversionCalculator
            );
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = MockSubstancesGenerator.Create(6);
            var activeSubstances = substances.Take(3).ToList();
            var biologicalMatrix = "Blood";
            var samplingMethod = FakeHbmDataGenerator.FakeHumanMonitoringSamplingMethod(biologicalMatrix);
            var hbmSampleSubstanceCollections = FakeHbmDataGenerator
                .FakeHbmSampleSubstanceCollections(individualDays, substances, samplingMethod);
            var result = calculator.Calculate(
                hbmSampleSubstanceCollections,
                individualDays
                    .Select(r => new IndividualDay() {
                        Individual = r.Individual,
                        IdDay = r.Day
                    })
                    .ToList(),
                activeSubstances,
                biologicalMatrix
            );
            var observedSubstances = result
                .SelectMany(r => r.ConcentrationsBySubstance.Keys)
                .Distinct()
                .ToList();
            CollectionAssert.AreEquivalent(activeSubstances, observedSubstances);
        }
    }
}
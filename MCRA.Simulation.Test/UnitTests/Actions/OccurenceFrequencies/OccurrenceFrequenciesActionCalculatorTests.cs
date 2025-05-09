﻿using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.OccurrenceFrequencies;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the AgriculturalUses action
    /// </summary>
    [TestClass]
    public class OccurrenceFrequenciesActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the OccurrenceFrequencies action: load data and summarize method
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesActionCalculator_TestLoad() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(2);
            var foods = FakeFoodsGenerator.Create(2);
            var occurrenceFrequencies = new List<OccurrenceFrequency> {
                new() {
                    Substance = substances[0],
                    Food = foods[0],
                    Percentage = 30,
                    Reference = "Ref0"
                },
                new() {
                    Substance = substances[1],
                    Food = foods[1],
                    Percentage = 40,
                    Reference = "Ref1"
                }
            };

            var compiledData = new CompiledData() {
                AllOccurrenceFrequencies = occurrenceFrequencies,
            };

            var dataManager = new MockCompiledDataManager(compiledData);
            var project = new ProjectDto();
            var subsetManager = new SubsetManager(dataManager, project);

            var data = new ActionData { };
            var config = project.OccurrencePatternsSettings;
            config.SetMissingAgriculturalUseAsUnauthorized = true;
            config.UseAgriculturalUsePercentage = true;
            var calculator = new OccurrenceFrequenciesActionCalculator(project);
            var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad");
            Assert.AreEqual(2, data.OccurrenceFractions.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestLoadAndSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, "TestLoad");
        }

        /// <summary>
        /// Runs the OccurrencePatterns module as compute
        /// config.SetMissingAgriculturalUseAsUnauthorized = true;
        ///  config.UseAgriculturalUsePercentage = true;
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesActionCalculator_TestOccurrenceFrequencies() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var foods = FakeFoodsGenerator.Create(2);
            var occurrencePatterns = new List<OccurrencePattern> {
                new() {
                    Code = "AU1",
                    Compounds = substances,
                    Food = foods[0],
                    StartDate = new DateTime(),
                    EndDate = new DateTime(),
                    Location = "Location1",
                    OccurrenceFraction = .8,
                },
                new() {
                    Code = "AU2",
                    Compounds = substances,
                    Food = foods[1],
                    StartDate = new DateTime(),
                    EndDate = new DateTime(),
                    Location = "Location2",
                    OccurrenceFraction = .8,
                }
            };
            var sampleOriginInfos = new Dictionary<Food, List<ISampleOrigin>> {
                [foods[0]] = [
                    new SampleOriginRecord { Food = foods[0], Location = "Location1", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = foods[0], Location = null, Fraction = 0F, NumberOfSamples = 0 }
                ],
                [foods[1]] = [
                    new SampleOriginRecord { Food = foods[1], Location = "Location2", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = foods[1], Location = null, Fraction = 0F, NumberOfSamples = 0 }
                ]
            };
            var marginalAgriculturalUsesCalculator = new MarginalOccurrencePatternsCalculator();
            var marginalOccurrencePatterns = marginalAgriculturalUsesCalculator.ComputeMarginalOccurrencePatterns(foods, occurrencePatterns, sampleOriginInfos);
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = substances,
                MarginalOccurrencePatterns = marginalOccurrencePatterns
            };
            var project = new ProjectDto();
            var config = project.OccurrencePatternsSettings;
            config.SetMissingAgriculturalUseAsUnauthorized = true;
            config.UseAgriculturalUsePercentage = true;
            config.RecomputeOccurrencePatterns = true;

            var calculatorNom = new OccurrenceFrequenciesActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "OccurrenceFrequenciesNom");

            var calculator = new OccurrenceFrequenciesActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);
            Assert.IsNotNull(data.OccurrenceFractions);
            Assert.AreEqual(6, data.OccurrenceFractions.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "OccurrenceFrequenciesUnc");
        }

        /// <summary>
        /// Runs the OccurrencePatterns module as compute
        /// config.SetMissingAgriculturalUseAsUnauthorized = true;
        ///  config.UseAgriculturalUsePercentage = true;
        /// </summary>
        [TestMethod]
        public void OccurrenceFrequenciesActionCalculator_TestOccurrenceFrequenciesRawAU() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var foods = FakeFoodsGenerator.Create(2);
            var occurrencePatterns = new List<OccurrencePattern> {
                new() {
                    Code = "AU1",
                    Compounds = substances,
                    Food = foods[0],
                    StartDate = new DateTime(),
                    EndDate = new DateTime(),
                    Location = "Location1",
                    OccurrenceFraction = .8,
                },
                new() {
                    Code = "AU2",
                    Compounds = substances,
                    Food = foods[1],
                    StartDate = new DateTime(),
                    EndDate = new DateTime(),
                    Location = "Location2",
                    OccurrenceFraction = .8,
                }
            };
            var sampleOriginInfos = new Dictionary<Food, List<ISampleOrigin>> {
                [foods[0]] = [
                    new SampleOriginRecord { Food = foods[0], Location = "Location1", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = foods[0], Location = null, Fraction = 0F, NumberOfSamples = 0 }
                ],
                [foods[1]] = [
                    new SampleOriginRecord { Food = foods[1], Location = "Location2", Fraction = 1F, NumberOfSamples = 5 },
                    new SampleOriginRecord { Food = foods[1], Location = null, Fraction = 0F, NumberOfSamples = 0 }
                ]
            };
            var data = new ActionData() {
                AllFoods = foods,
                ActiveSubstances = substances,
                RawAgriculturalUses = occurrencePatterns,
                SampleOriginInfos = sampleOriginInfos
            };
            var project = new ProjectDto();
            var config = project.OccurrencePatternsSettings;
            config.SetMissingAgriculturalUseAsUnauthorized = true;
            config.UseAgriculturalUsePercentage = true;
            config.RecomputeOccurrencePatterns = true;

            var calculatorNom = new OccurrenceFrequenciesActionCalculator(project);
            _ = TestRunUpdateSummarizeNominal(project, calculatorNom, data, "OccurrenceFrequenciesOccurencePatternsNom");

            var calculator = new OccurrenceFrequenciesActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, null);

            Assert.IsNotNull(data.OccurrenceFractions);
            Assert.AreEqual(6, data.OccurrenceFractions.Count);

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators, reportFileName: "OccurrenceFrequenciesOccurencePatternsUnc");
        }
    }
}
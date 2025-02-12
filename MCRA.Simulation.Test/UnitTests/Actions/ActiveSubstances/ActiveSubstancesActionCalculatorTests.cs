﻿using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActiveSubstances;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the AssessmentGroupMemberships action
    /// </summary>
    [TestClass]
    public class ActiveSubstancesActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the ActiveSubstances action: load data and summarize method, several scenarios
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesActionCalculator_TestLoadDataScenarios() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = FakeSubstancesGenerator.Create(8);
            var hazardDoses = FakePointsOfDepartureGenerator
                .Create(substances.Take(4), PointOfDepartureType.Bmd, focalEffect, "Rat", random);
            var availableAssessmentGroupMembershipModels = new List<ActiveSubstanceModel>() {
                FakeAssessmentGroupMembershipModelsGenerator.Create(
                    focalEffect,
                    substances,
                    [0D, .5, 1D, double.NaN, 0D, .5, 1D, double.NaN]
                )
            };

            var compiledData = new CompiledData() {
                AllActiveSubstanceModels = availableAssessmentGroupMembershipModels.ToDictionary(r => r.Code),
            };
            var dataManager = new MockCompiledDataManager(compiledData);

            var intersection = CombinationMethodMembershipInfoAndPodPresence.Intersection;
            var union = CombinationMethodMembershipInfoAndPodPresence.Union;
            var scenarios = new List<(ProjectDto Project, int Count)>() {
                (createScenarioProject(false, intersection, false, false, false), 4),
                (createScenarioProject(false, intersection, false, true, false), 2),
                (createScenarioProject(false, intersection, true, false, false), 6),
                (createScenarioProject(true, union, false, false, false), 6),
                (createScenarioProject(true, union, false, true, false), 5),
                (createScenarioProject(true, union, true, false, false), 7),
                (createScenarioProject(true, intersection, false, false, false), 2),
                (createScenarioProject(true, intersection, false, true, false), 1),
                (createScenarioProject(true, intersection, true, false, false), 3),
            };
            var count = 0;
            foreach (var (project, activeCount) in scenarios) {
                count++;
                var data = new ActionData {
                    AllCompounds = substances,
                    SelectedEffect = effects.First(),
                    RelevantEffects = effects,
                    PointsOfDeparture = hazardDoses.Values,
                };
                var subsetManager = new SubsetManager(dataManager, project);
                var calculator = new ActiveSubstancesActionCalculator(project);
                var header = TestLoadAndSummarizeNominal(calculator, data, subsetManager, $"TestLoad1_{count}");

                Assert.IsNotNull(data.MembershipProbabilities);
                Assert.AreEqual(activeCount, data.ActiveSubstances.Count);
                Assert.IsNotNull(data.AvailableActiveSubstanceModels);
                Assert.IsNotNull(data.ActiveSubstances);
            }
        }

        /// <summary>
        /// Runs the ActiveSubstances action: load data and summarize method, several scenarios
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesActionCalculator_TestComputeScenarios() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effects = FakeEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = FakeSubstancesGenerator.Create(10);
            var pointsOfDeparture = FakePointsOfDepartureGenerator.Create(substances.Take(5), PointOfDepartureType.Bmd, focalEffect, "Rat", random).Select(c => c.Value).ToList();
            var availableQsarModels = new List<QsarMembershipModel>() {
                FakeQsarMembershipModelsGenerator.Create(
                    focalEffect,
                    substances,
                    [0D, 0D, 1D, 1D, double.NaN, 0D, 0D, 1D, 1D, double.NaN]
                )
            };
            var availableDockingModels = new List<MolecularDockingModel>() {
                FakeMolecularDockingModelsGenerator.Create(
                    focalEffect,
                    substances,
                    -6,
                    [-5, -7, -5, -7, double.NaN, -5, -7, -5, -7, double.NaN]
                )
            };

            var crispMax = AssessmentGroupMembershipCalculationMethod.CrispMax;
            var intersection = CombinationMethodMembershipInfoAndPodPresence.Intersection;
            var union = CombinationMethodMembershipInfoAndPodPresence.Union;
            var scenarios = new List<(ProjectDto, int)>() {
                (createScenarioProject(true, intersection, false, false, false, crispMax, true, true), 3),
                (createScenarioProject(true, intersection, false, true, false, crispMax, true, true), 3),
                (createScenarioProject(true, intersection, true, false, false, crispMax, true, true), 4),
                (createScenarioProject(true, union, false, false, false, crispMax, true, true), 8),
                (createScenarioProject(true, union, false, true, false, crispMax, true, true), 8),
                (createScenarioProject(true, union, true, false, false, crispMax, true, true), 9),
                (createScenarioProject(false, intersection, false, false, false, crispMax, true, true), 6),
                (createScenarioProject(false, intersection, false, true, false, crispMax, true, true), 6),
                (createScenarioProject(false, intersection, true, false, false, crispMax, true, true), 8),
                (createScenarioProject(true, union, false, true, false, crispMax, false, false), 5),
                (createScenarioProject(true, intersection, false, true, false, crispMax, false, false), 5),
                (createScenarioProject(false, union, false, true, false, crispMax, false, false), 10),
                (createScenarioProject(false, intersection, false, true, false, crispMax, false, false), 10),
            };
            var count = 0;
            foreach (var (project, activeCount) in scenarios) {
                count++;
                var data = new ActionData {
                    AllCompounds = substances,
                    SelectedEffect = effects.First(),
                    RelevantEffects = effects,
                    PointsOfDeparture = pointsOfDeparture,
                    QsarMembershipModels = availableQsarModels,
                    MolecularDockingModels = availableDockingModels,
                };
                var calculator = new ActiveSubstancesActionCalculator(project);
                var header = TestRunUpdateSummarizeNominal(new ProjectDto(), calculator, data, $"AssessmentGroupMemberships_{count}");
                Assert.IsNotNull(data.MembershipProbabilities);
                Assert.AreEqual(activeCount, data.ActiveSubstances.Count);
            }
        }

        private static ProjectDto createScenarioProject(
            bool restrictToAvailableHazardDoses,
            CombinationMethodMembershipInfoAndPodPresence combineMethod,
            bool includeSubstancesWithUnknowMemberships,
            bool restrictToCertainMembership,
            bool useProbabilisticMemberships,
            AssessmentGroupMembershipCalculationMethod assessmentGroupMembershipCalculationMethod = AssessmentGroupMembershipCalculationMethod.CrispMax,
            bool useQsarModels = true,
            bool useMolecularDockingModels = true
        ) {

            var config = new ActiveSubstancesModuleConfig {
                FilterByAvailableHazardDose = restrictToAvailableHazardDoses,
                CombinationMethodMembershipInfoAndPodPresence = combineMethod,
                UseProbabilisticMemberships = useProbabilisticMemberships,
                FilterByCertainAssessmentGroupMembership = restrictToCertainMembership,
                IncludeSubstancesWithUnknowMemberships = includeSubstancesWithUnknowMemberships,
                UseQsarModels = useQsarModels,
                UseMolecularDockingModels = useMolecularDockingModels,
                AssessmentGroupMembershipCalculationMethod = assessmentGroupMembershipCalculationMethod
            };

            var project = new ProjectDto(config);

            return project;
        }

        /// <summary>
        ///  Runs the ActiveSubstances action: load data and summarize method
        ///  project.EffectSettings.UseProbabilisticMemberships = true;
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesActionCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = new Effect() { Code = "code" };
            var substances = FakeSubstancesGenerator.Create(3);
            var pointsOfDeparture = FakePointsOfDepartureGenerator.Create(substances, PointOfDepartureType.Bmd, effect, "Rat", random).Select(c => c.Value).ToList();
            var relevantEffects = new List<Effect>() { effect };
            var rpfDictionary = new Dictionary<string, List<RelativePotencyFactor>> {
                [effect.Code] = substances
                .Select(c => new RelativePotencyFactor() { Compound = c, Effect = effect, RPF = 1 })
                .ToList()
            };
            var correctedRelativePotencyFactors = rpfDictionary.SelectMany(c => c.Value).ToDictionary(c => c.Compound);

            var project = new ProjectDto();
            project.ActiveSubstancesSettings.UseProbabilisticMemberships = true;
            project.ActiveSubstancesSettings.IsCompute = true;
            var data = new ActionData() {
                AllCompounds = substances,
                SelectedEffect = effect,
                PointsOfDeparture = pointsOfDeparture,
                RelevantEffects = relevantEffects,
                RawRelativePotencyFactors = correctedRelativePotencyFactors
            };

            var calculator = new ActiveSubstancesActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestCompute");
            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.AssessmentGroupMemberships);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Runs the ActiveSubstances action: run and summarize method
        /// project.EffectSettings.UseProbabilisticMemberships = false;
        /// </summary>
        [TestMethod]
        public void ActiveSubstancesActionCalculator_TestComputeUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = new Effect() { Code = "code" };
            var substances = FakeSubstancesGenerator.Create(3);
            var pointsOfDeparture = FakePointsOfDepartureGenerator
                .Create(substances, PointOfDepartureType.Bmd, effect, "Rat", random);
            var relevantEffects = new List<Effect>() { effect };

            var data = new ActionData() {
                AllCompounds = substances,
                SelectedEffect = effect,
                PointsOfDeparture = pointsOfDeparture.Values,
                RelevantEffects = relevantEffects
            };

            var project = new ProjectDto();
            project.ActiveSubstancesSettings.UseProbabilisticMemberships = false;
            project.ActiveSubstancesSettings.IsCompute = true;

            var calculator = new ActiveSubstancesActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestComputeUncertain");
            Assert.IsNotNull(data.AvailableActiveSubstanceModels);
            Assert.IsNotNull(data.ActiveSubstances);

            for (int i = 0; i < 10; i++) {
                calculator = new ActiveSubstancesActionCalculator(project);
                var factorialSet = new UncertaintyFactorialSet(UncertaintySource.AssessmentGroupMemberships);
                var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
                var resultUnc = calculator.RunUncertain(data, factorialSet, uncertaintySourceGenerators, header, new CompositeProgressState());
                TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            }

            WriteReport(header, "TestComputeUncertain.html");
        }
    }
}
using System.Reflection;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.HazardCharacterisations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {

    /// <summary>
    /// Runs the HazardCharacterisations action
    /// </summary>
    [TestClass]
    public class HazardCharacterisationsActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the load data method of the hazard characterisation action. Loads
        /// hazard characterisations from compiled data. Exposure type is acute.
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsActionCalculator_TestLoadData() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);

            var effect = FakeEffectsGenerator.Create();
            var substances = FakeSubstancesGenerator.Create(10);
            var responses = FakeResponsesGenerator.Create(1);
            var routes = new[] {
                ExposureRoute.Dermal,
                ExposureRoute.Oral,
                ExposureRoute.Inhalation
            };

            var data = new ActionData {
                AllCompounds = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = FakeAbsorptionFactorsGenerator
                    .Create(routes, substances),
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            var exposureType = ExposureType.Acute;
            var config = project.HazardCharacterisationsSettings;
            config.ExposureType = exposureType;
            config.ExposureRoutes = [.. routes];
            var compiledData = new CompiledData() {
                AllHazardCharacterisations = FakeHazardCharacterisationsGenerator
                    .CreateExternal(substances, effect, exposureType, isCriticalEffect: false, seed: seed)
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.HazardCharacterisationModelsCollections);
            Assert.AreEqual(10, data.HazardCharacterisationModelsCollections.SelectMany(c => c.HazardCharacterisationModels).Count());
        }

        /// <summary>
        /// Runs the load data method of the hazard characterisation action. Loads
        /// hazard characterisations from compiled data. Exposure type is acute.
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsActionCalculator_TestLoadDataCriticalEffect() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);

            var substances = FakeSubstancesGenerator.Create(10);
            var responses = FakeResponsesGenerator.Create(1);
            var routes = new[] {
                ExposureRoute.Dermal,
                ExposureRoute.Oral,
                ExposureRoute.Inhalation
            };

            var data = new ActionData {
                AllCompounds = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = FakeAbsorptionFactorsGenerator.Create(routes, substances),
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            var config = project.HazardCharacterisationsSettings;
            var exposureType = ExposureType.Acute;
            config.ExposureType = exposureType;
            config.ExposureRoutes = routes.Select(r => r).ToList();
            config.RestrictToCriticalEffect = true;
            var compiledData = new CompiledData() {
                AllHazardCharacterisations = FakeHazardCharacterisationsGenerator
                    .CreateExternal(substances, null, exposureType, seed: seed)
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad2");
            Assert.IsNotNull(data.HazardCharacterisationModelsCollections);
            Assert.AreEqual(10, data.HazardCharacterisationModelsCollections.SelectMany(c => c.HazardCharacterisationModels.Values).Count());
        }

        [TestMethod]
        [DataRow(ExposureType.Acute)]
        [DataRow(ExposureType.Chronic)]
        public void LoadData_ExternalHazardDoses_ShouldYieldCorrectPerBWUnit(ExposureType exposureType) {
            int seed = 1;
            var random = new McraRandomGenerator(seed);

            var effect = FakeEffectsGenerator.Create();
            var substances = FakeSubstancesGenerator.Create(10);
            var responses = FakeResponsesGenerator.Create(1);
            var routes = new List<ExposureRoute>() { ExposureRoute.Oral };

            var data = new ActionData {
                AllCompounds = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = FakeAbsorptionFactorsGenerator.Create(routes, substances),
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            var config = project.HazardCharacterisationsSettings;
            config.ExposureType = exposureType;

            var compiledData = new CompiledData() {
                AllHazardCharacterisations = FakeHazardCharacterisationsGenerator
                    .CreateExternal(substances, effect, exposureType, isCriticalEffect: false, seed: seed)
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, MethodBase.GetCurrentMethod().Name);
            Assert.AreEqual(TargetLevelType.External, data.HazardCharacterisationModelsCollections.First().TargetUnit.TargetLevelType);
            Assert.AreEqual(ExposureRoute.Oral, data.HazardCharacterisationModelsCollections.First().TargetUnit.ExposureRoute);
            Assert.IsTrue(data.HazardCharacterisationModelsCollections.First().TargetUnit.IsPerBodyWeight);
        }

        /// <summary>
        /// Runs the HazardCharacterisations action: run, summarize action result, update simulation data, run uncertain,
        /// summarize action result uncertain, update simulation data uncertain
        /// TargetDosesCalculationMethod = TargetDosesCalculationMethod.InVitroBmds
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsActionCalculator_TestNone() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);

            var routes = new[] {
                ExposureRoute.Dermal,
                ExposureRoute.Oral,
                ExposureRoute.Inhalation
            };
            var effect = new Effect() { Code = "effect" };
            var substances = FakeSubstancesGenerator.Create(3);
            var species = "Rat";

            var data = new ActionData {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                KineticConversionFactorModels = [],
                PointsOfDeparture = FakePointsOfDepartureGenerator
                    .Create(substances, PointOfDepartureType.Bmd, effect, species, random)
                    .Select(c => c.Value)
                    .ToList(),
                InterSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator
                    .Create(substances, [species], effect, random),
                IntraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator.Create(substances),
                KineticModelInstances = [],
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            var config = project.HazardCharacterisationsSettings;
            config.TargetDoseSelectionMethod = TargetDoseSelectionMethod.MostToxic;
            config.ImputeMissingHazardDoses = true;
            config.ExposureRoutes = [ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation];
            config.TargetDosesCalculationMethod = TargetDosesCalculationMethod.InVivoPods;
            config.UseAdditionalAssessmentFactor = true;
            config.AdditionalAssessmentFactor = 100;
            config.InternalModelType = InternalModelType.PBKModel;
            project.HazardCharacterisationsSettings.IsCompute = true;

            var calculator = new HazardCharacterisationsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestNone");
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.RPFs] = random
            };
            var factorialSet = new UncertaintyFactorialSet() { UncertaintySources = [] };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Runs the HazardCharacterisations action: run, summarize action result, update simulation data, run uncertain,
        /// summarize action result uncertain, update simulation data uncertain
        /// HazardCharacterisationsCalculationMethod = HazardCharacterisationsCalculationMethod.CombineInVivoPodInVitroDrms
        /// </summary>
        [TestMethod]
        public void HazardCharacterisationsActionCalculator_TestIVIVE() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);

            var effect = FakeEffectsGenerator.Create();
            var substances = FakeSubstancesGenerator.Create(3);
            var responses = FakeResponsesGenerator.Create(1);
            var response = responses.First();
            var species = "Rat";
            var routes = new List<ExposureRoute> {
                ExposureRoute.Dermal,
                ExposureRoute.Oral,
                ExposureRoute.Inhalation
            };
            var internalDoseUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.Liver);

            var kineticConversionFactorModels = FakeKineticModelsGenerator
                .CreateKineticConversionFactorModels(substances, routes, internalDoseUnit);

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                SelectedEffect = effect,
                PointsOfDeparture = FakePointsOfDepartureGenerator
                    .Create(substances, PointOfDepartureType.Bmd, effect, species, random)
                    .Select(c => c.Value)
                    .ToList(),
                FocalEffectRepresentations = FakeEffectRepresentationsGenerator
                    .Create([effect], responses),
                DoseResponseModels = FakeDoseResponseModelGenerator
                    .Create([substances.First()], responses, random),
                InterSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator
                    .Create(substances, [species], effect, random),
                IntraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator
                    .Create(substances),
                KineticModelInstances = [],
                SelectedPopulation = new Population { NominalBodyWeight = 70 },
                KineticConversionFactorModels = kineticConversionFactorModels
            };

            var project = new ProjectDto();
            project.HazardCharacterisationsSettings.IsCompute = true;
            var config = project.HazardCharacterisationsSettings;
            config.ApplyKineticConversions = true;
            config.CodeReferenceSubstance = substances.First().Code;
            config.TargetDoseSelectionMethod = TargetDoseSelectionMethod.MostToxic;
            config.ImputeMissingHazardDoses = true;
            config.TargetDosesCalculationMethod = TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms;
            config.ExposureRoutes = [ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation];
            config.InternalModelType = InternalModelType.PBKModel;
            var calculator = new HazardCharacterisationsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeIVIVE");
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.RPFs] = random
            };
            var factorialSet = new UncertaintyFactorialSet() { UncertaintySources = [] };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        [TestMethod]
        public void Run_InternalFromPodForDifferentExposureTargets_ShouldYieldCollectionsPerExposureTargets() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);

            var effect = FakeEffectsGenerator.Create();

            var responses = FakeResponsesGenerator.Create(1, null, TestSystemType.InVivo);
            var response = responses.First();

            var species = "Rat";
            var routes = new[] {
                ExposureRoute.Dermal,
                ExposureRoute.Oral,
                ExposureRoute.Inhalation
            };
            var targetLevel = TargetLevelType.Internal;
            var exposureType = ExposureType.Chronic;

            var substancesBlood = FakeSubstancesGenerator.Create(["S1", "S2", "S3"]);
            var substancesBloodLipids = FakeSubstancesGenerator.Create(["S4", "S5", "S6"]);
            var substancesUrine = FakeSubstancesGenerator.Create(["S7", "S8"]);
            // Add two duplicate substances, both present in blood and in urine
            substancesUrine.Add(substancesBlood[0]);
            substancesUrine.Add(substancesBlood[1]);

            var substances = new HashSet<Compound>();
            substances.UnionWith(substancesBlood);
            substances.UnionWith(substancesBloodLipids);
            substances.UnionWith(substancesUrine);

            var exposureTargetBlood = new ExposureTarget(BiologicalMatrix.Blood, ExpressionType.None);
            var exposureTargetBloodLipids = new ExposureTarget(BiologicalMatrix.Blood, ExpressionType.Lipids);
            var exposureTargetUrine = new ExposureTarget(BiologicalMatrix.Urine, ExpressionType.None);

            var podBlood = FakePointsOfDepartureGenerator.Create(substancesBlood, PointOfDepartureType.Bmd, effect, species, random, exposureTargetBlood, targetLevel, DoseUnit.ugPerL);
            var podBloodLipids = FakePointsOfDepartureGenerator.Create(substancesBloodLipids, PointOfDepartureType.Bmd, effect, species, random, exposureTargetBloodLipids, targetLevel, DoseUnit.ugPerg);
            var podBloodUrine = FakePointsOfDepartureGenerator.Create(substancesUrine, PointOfDepartureType.Bmd, effect, species, random, exposureTargetUrine, targetLevel, DoseUnit.ugPerL);
            var allPointsOfDeparture = new List<Data.Compiled.Objects.PointOfDeparture>();
            allPointsOfDeparture.AddRange(podBlood.Select(p => p.Value).ToList());
            allPointsOfDeparture.AddRange(podBloodLipids.Select(p => p.Value).ToList());
            allPointsOfDeparture.AddRange(podBloodUrine.Select(p => p.Value).ToList());

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = FakeAbsorptionFactorsGenerator.Create(routes, substances),
                SelectedEffect = effect,
                PointsOfDeparture = allPointsOfDeparture,
                FocalEffectRepresentations = FakeEffectRepresentationsGenerator
                    .Create([effect], responses),
                InterSpeciesFactorModels = FakeInterSpeciesFactorModelsGenerator
                    .Create(substances, [species], effect, random),
                IntraSpeciesFactorModels = FakeIntraSpeciesFactorModelsGenerator
                    .Create(substances),
                KineticModelInstances = [],
                KineticConversionFactorModels = [],
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            project.HazardCharacterisationsSettings.IsCompute = true;
            var config = project.HazardCharacterisationsSettings;
            config.CodeReferenceSubstance = substances.First().Code;
            config.TargetDoseSelectionMethod = TargetDoseSelectionMethod.MostToxic;
            config.TargetDoseLevelType = targetLevel;
            config.TargetDosesCalculationMethod = TargetDosesCalculationMethod.InVivoPods;
            config.ExposureRoutes = [ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation];
            config.ExposureType = exposureType;
            config.InternalModelType = InternalModelType.PBKModel;
            var calculator = new HazardCharacterisationsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestFromPod");
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.RPFs] = random
            };
            var factorialSet = new UncertaintyFactorialSet() { UncertaintySources = [] };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Test conversion of hazard characterisation dose values for different internal expression types (lipid and creatinine standardisation)
        /// </summary>
        [DataRow(BiologicalMatrix.Blood, ExpressionType.None, DoseUnit.ugPerL, 1)]              // The system default target unit for aqueous is ugPerL
        [DataRow(BiologicalMatrix.Blood, ExpressionType.None, DoseUnit.mgPerL, 1000)]           // The system default target unit for aqueous is ugPerL
        [DataRow(BiologicalMatrix.Blood, ExpressionType.Lipids, DoseUnit.ngPerg, 0.001)]        // The system default target unit for standardisation is ugPerg
        [DataRow(BiologicalMatrix.Blood, ExpressionType.Lipids, DoseUnit.ugPerg, 1)]            // The system default target unit for standardisation is ugPerg
        [DataRow(BiologicalMatrix.Blood, ExpressionType.Lipids, DoseUnit.mgPerg, 1000)]         // The system default target unit for standardisation is ugPerg
        [DataRow(BiologicalMatrix.Urine, ExpressionType.Creatinine, DoseUnit.ugPerg, 1)]        // The system default target unit for standardisation is ugPerg
        [DataRow(BiologicalMatrix.Urine, ExpressionType.Creatinine, DoseUnit.mgPerKg, 1)]       // The system default target unit for standardisation is ugPerg
        [DataRow(BiologicalMatrix.Urine, ExpressionType.Creatinine, DoseUnit.ugPerKg, 0.001)]   // The system default target unit for standardisation is ugPerg
        [TestMethod]
        public void LoadData_DifferentExpressionTypes_ShouldApplyCorrectDoseUnitAlignmentFactor(
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType,
            DoseUnit doseUnit,
            double expectedAlignmentFactor
        ) {
            var seed = 1;
            var effect = FakeEffectsGenerator.Create();
            var substances = FakeSubstancesGenerator.Create(10);

            var data = new ActionData {
                AllCompounds = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            var config = project.HazardCharacterisationsSettings;
            var exposureType = ExposureType.Chronic;
            config.ExposureType = exposureType;
            config.TargetDoseLevelType = TargetLevelType.Internal;

            var compiledData = new CompiledData() {
                AllHazardCharacterisations = FakeHazardCharacterisationsGenerator
                    .CreateInternal(
                        substances,
                        effect,
                        exposureType,
                        biologicalMatrix,
                        expressionType,
                        doseUnit,
                        seed: seed
                    )
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, nameof(LoadData_DifferentExpressionTypes_ShouldApplyCorrectDoseUnitAlignmentFactor));

            Assert.IsNotNull(data.HazardCharacterisationModelsCollections);
            Assert.AreEqual(1, data.HazardCharacterisationModelsCollections.Count);
            var hazardCharacterisationModels = data.HazardCharacterisationModelsCollections.First().HazardCharacterisationModels;

            var valuesIn = compiledData.AllHazardCharacterisations.Select(h => h.Value).ToList();
            var valuesOut = hazardCharacterisationModels.Select(m => m.Value.Value).ToList();
            var combinedSequence = valuesIn.Zip(valuesOut);
            foreach (var (valueIn, valueOut) in combinedSequence) {
                Assert.AreEqual(Math.Round(valueIn * expectedAlignmentFactor, 4), Math.Round(valueOut, 4));
            }
        }
    }
}

using System.Reflection;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.HazardCharacterisations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
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

            var effect = MockEffectsGenerator.Create();
            var substances = MockSubstancesGenerator.Create(10);
            var responses = MockResponsesGenerator.Create(1);
            var exposureRoutes = new List<ExposurePathType>() {
                ExposurePathType.Dermal,
                ExposurePathType.Oral,
                ExposurePathType.Inhalation
            };

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances),
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            var exposureType = ExposureType.Acute;
            project.AssessmentSettings.ExposureType = exposureType;

            var compiledData = new CompiledData() {
                AllHazardCharacterisations = MockHazardCharacterisationsGenerator
                    .Create(substances, effect, exposureType, 100, ExposurePathType.Oral, false, seed)
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

            var substances = MockSubstancesGenerator.Create(10);
            var responses = MockResponsesGenerator.Create(1);
            var exposureRoutes = new List<ExposurePathType>() {
                ExposurePathType.Dermal,
                ExposurePathType.Oral,
                ExposurePathType.Inhalation
            };

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances),
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            var exposureType = ExposureType.Acute;
            project.AssessmentSettings.ExposureType = exposureType;
            project.EffectSettings.RestrictToCriticalEffect = true;
            var compiledData = new CompiledData() {
                AllHazardCharacterisations = MockHazardCharacterisationsGenerator
                    .Create(substances, null, exposureType, 100, ExposurePathType.Oral, true, seed)
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

            var effect = MockEffectsGenerator.Create();
            var substances = MockSubstancesGenerator.Create(10);
            var responses = MockResponsesGenerator.Create(1);
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.Oral };

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances),
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            project.AssessmentSettings.ExposureType = exposureType;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.External;

            var compiledData = new CompiledData() {
                AllHazardCharacterisations = MockHazardCharacterisationsGenerator
                    .Create(substances, effect, exposureType, 100, ExposurePathType.Oral, false, seed)
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, MethodBase.GetCurrentMethod().Name);
            Assert.AreEqual(TargetLevelType.External, data.HazardCharacterisationModelsCollections.First().TargetUnit.TargetLevelType);
            Assert.AreEqual(ExposureRoute.Oral, data.HazardCharacterisationModelsCollections.First().TargetUnit.ExposureRoute);
            Assert.IsTrue(data.HazardCharacterisationModelsCollections.First().TargetUnit.IsPerBodyWeight());
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

            var exposureRoutes = new List<ExposurePathType>() {
                ExposurePathType.Dermal,
                ExposurePathType.Oral,
                ExposurePathType.Inhalation
            };
            var effect = new Effect() { Code = "effect" };
            var substances = MockSubstancesGenerator.Create(3);
            var species = "Rat";
            var data = new ActionData {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances),
                PointsOfDeparture = MockPointsOfDepartureGenerator
                    .Create(substances, PointOfDepartureType.Bmd, effect, species, random)
                    .Select(c => c.Value)
                    .ToList(),
                InterSpeciesFactorModels = MockInterSpeciesFactorModelsGenerator
                    .Create(substances, new List<string>() { species }, effect, random),
                IntraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator.Create(substances),
                KineticModelInstances = new List<KineticModelInstance>(),
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            project.EffectSettings.TargetDoseSelectionMethod = TargetDoseSelectionMethod.MostToxic;
            project.EffectSettings.ImputeMissingHazardDoses = true;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.Aggregate = true;
            project.EffectSettings.TargetDosesCalculationMethod = TargetDosesCalculationMethod.InVivoPods;
            project.EffectSettings.UseAdditionalAssessmentFactor = true;
            project.EffectSettings.AdditionalAssessmentFactor = 100;
            project.CalculationActionTypes.Add(ActionType.HazardCharacterisations);

            var calculator = new HazardCharacterisationsActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestNone");
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.RPFs] = random
            };
            var factorialSet = new UncertaintyFactorialSet() { UncertaintySources = new List<UncertaintySource>() };
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

            var effect = MockEffectsGenerator.Create();
            var substances = MockSubstancesGenerator.Create(3);
            var responses = MockResponsesGenerator.Create(1);
            var response = responses.First();
            var species = "Rat";
            var exposureRoutes = new List<ExposurePathType>() {
                ExposurePathType.Dermal,
                ExposurePathType.Oral,
                ExposurePathType.Inhalation
            };

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances),
                SelectedEffect = effect,
                PointsOfDeparture = MockPointsOfDepartureGenerator
                    .Create(substances, PointOfDepartureType.Bmd, effect, species, random)
                    .Select(c => c.Value)
                    .ToList(),
                FocalEffectRepresentations = MockEffectRepresentationsGenerator
                    .Create(new List<Effect> { effect }, responses),
                DoseResponseModels = MockDoseResponseModelGenerator
                    .Create(new List<Compound> { substances.First() }, responses, random),
                InterSpeciesFactorModels = MockInterSpeciesFactorModelsGenerator
                    .Create(substances, new List<string>() { species }, effect, random),
                IntraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator
                    .Create(substances),
                KineticModelInstances = new List<KineticModelInstance>(),
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            project.EffectSettings.CodeReferenceCompound = substances.First().Code;
            project.EffectSettings.TargetDoseSelectionMethod = TargetDoseSelectionMethod.MostToxic;
            project.EffectSettings.ImputeMissingHazardDoses = true;
            project.EffectSettings.TargetDosesCalculationMethod = TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.Aggregate = true;
            var calculator = new HazardCharacterisationsActionCalculator(project);
            project.CalculationActionTypes.Add(ActionType.HazardCharacterisations);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestComputeIVIVE");
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.RPFs] = random
            };
            var factorialSet = new UncertaintyFactorialSet() { UncertaintySources = new List<UncertaintySource>() };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        [TestMethod]
        public void Run_InternalFromPodForDifferentExposureTargets_ShouldYieldCollectionsPerExposureTargets() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);

            var effect = MockEffectsGenerator.Create();

            var responses = MockResponsesGenerator.Create(1, null, TestSystemType.InVivo);
            var response = responses.First();

            var species = "Rat";
            var exposureRoutes = new List<ExposurePathType>() {
                ExposurePathType.Dermal,
                ExposurePathType.Oral,
                ExposurePathType.Inhalation
            };
            var targetLevel = TargetLevelType.Internal;
            var exposureType = ExposureType.Chronic;

            var substancesBlood = MockSubstancesGenerator.Create(new[] { "S1", "S2", "S3" });
            var substancesBloodLipids = MockSubstancesGenerator.Create(new[] { "S4", "S5", "S6" });
            var substancesUrine = MockSubstancesGenerator.Create(new[] { "S7", "S8" });
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

            var podBlood = MockPointsOfDepartureGenerator.Create(substancesBlood, PointOfDepartureType.Bmd, effect, species, random, exposureTargetBlood, targetLevel, "ug/L");
            var podBloodLipids = MockPointsOfDepartureGenerator.Create(substancesBloodLipids, PointOfDepartureType.Bmd, effect, species, random, exposureTargetBloodLipids, targetLevel, "µg/g");
            var podBloodUrine = MockPointsOfDepartureGenerator.Create(substancesUrine, PointOfDepartureType.Bmd, effect, species, random, exposureTargetUrine, targetLevel, "ug/L");
            var allPointsOfDeparture = new List<Data.Compiled.Objects.PointOfDeparture>();
            allPointsOfDeparture.AddRange(podBlood.Select(p => p.Value).ToList());
            allPointsOfDeparture.AddRange(podBloodLipids.Select(p => p.Value).ToList());
            allPointsOfDeparture.AddRange(podBloodUrine.Select(p => p.Value).ToList());

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances),
                SelectedEffect = effect,
                PointsOfDeparture = allPointsOfDeparture,
                FocalEffectRepresentations = MockEffectRepresentationsGenerator
                    .Create(new List<Effect> { effect }, responses),
                InterSpeciesFactorModels = MockInterSpeciesFactorModelsGenerator
                    .Create(substances, new List<string>() { species }, effect, random),
                IntraSpeciesFactorModels = MockIntraSpeciesFactorModelsGenerator
                    .Create(substances),
                KineticModelInstances = new List<KineticModelInstance>(),
                SelectedPopulation = new Population { NominalBodyWeight = 70 }
            };

            var project = new ProjectDto();
            project.EffectSettings.CodeReferenceCompound = substances.First().Code;
            project.EffectSettings.TargetDoseSelectionMethod = TargetDoseSelectionMethod.MostToxic;
            project.EffectSettings.TargetDoseLevelType = targetLevel;
            project.EffectSettings.TargetDosesCalculationMethod = TargetDosesCalculationMethod.InVivoPods;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.Aggregate = true;
            project.AssessmentSettings.ExposureType = exposureType;
            var calculator = new HazardCharacterisationsActionCalculator(project);
            project.CalculationActionTypes.Add(ActionType.HazardCharacterisations);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, "TestFromPod");
            var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom> {
                [UncertaintySource.RPFs] = random
            };
            var factorialSet = new UncertaintyFactorialSet() { UncertaintySources = new List<UncertaintySource>() };
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
        }

        /// <summary>
        /// Test conversion of hazard characterisation dose values for different internal expression types (lipid and creatinine standardisation)
        /// </summary>
        [DataRow(ExpressionType.None, DoseUnit.ugPerL, 1)]              // The system default target unit for aqueous is ugPerL
        [DataRow(ExpressionType.None, DoseUnit.mgPerL, 1000)]           // The system default target unit for aqueous is ugPerL
        [DataRow(ExpressionType.Lipids, DoseUnit.ngPerg, 0.001)]        // The system default target unit for standardisation is ugPerg
        [DataRow(ExpressionType.Lipids, DoseUnit.ugPerg, 1)]            // The system default target unit for standardisation is ugPerg
        [DataRow(ExpressionType.Lipids, DoseUnit.mgPerg, 1000)]         // The system default target unit for standardisation is ugPerg
        [DataRow(ExpressionType.Creatinine, DoseUnit.ugPerg, 1)]        // The system default target unit for standardisation is ugPerg
        [DataRow(ExpressionType.Creatinine, DoseUnit.mgPerKg, 1)]       // The system default target unit for standardisation is ugPerg
        [DataRow(ExpressionType.Creatinine, DoseUnit.ugPerKg, 0.001)]   // The system default target unit for standardisation is ugPerg
        [TestMethod]
        public void LoadData_DifferentExpressionTypes_ShouldApplyCorrectoseUnitAlignmentFactor(ExpressionType expressionType, DoseUnit doseUnit, double expectedAlignmentFactor) {
            var effect = MockEffectsGenerator.Create();
            var substances = MockSubstancesGenerator.Create(10);
            var exposureRoutes = new List<ExposurePathType>() { ExposurePathType.AtTarget };

            var data = new ActionData {
                ActiveSubstances = substances,
                MembershipProbabilities = substances.ToDictionary(r => r, r => 1d),
                AbsorptionFactors = MockAbsorptionFactorsGenerator.Create(exposureRoutes, substances),
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
            };

            var project = new ProjectDto();
            var exposureType = ExposureType.Chronic;
            project.AssessmentSettings.ExposureType = exposureType;
            project.EffectSettings.TargetDoseLevelType = TargetLevelType.Internal;

            var biologicalMatrix = expressionType == ExpressionType.Creatinine ? BiologicalMatrix.Urine : BiologicalMatrix.Blood;
            var compiledData = new CompiledData() {
                AllHazardCharacterisations = MockHazardCharacterisationsGenerator
                    .Create(substances, effect, exposureType, 100, ExposurePathType.AtTarget, TargetLevelType.Internal, doseUnit, biologicalMatrix, expressionType)
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, nameof(LoadData_DifferentExpressionTypes_ShouldApplyCorrectoseUnitAlignmentFactor));

            Assert.IsNotNull(data.HazardCharacterisationModelsCollections);
            Assert.AreEqual(1, data.HazardCharacterisationModelsCollections.Count);
            var hazardCharacterisationModels = data.HazardCharacterisationModelsCollections.First().HazardCharacterisationModels;

            var valuesIn = compiledData.AllHazardCharacterisations.Select(h => h.Value).ToList();
            var valuesOut = hazardCharacterisationModels.Select(m => m.Value.Value).ToList();
            var combinedSequence = valuesIn.Zip(valuesOut);
            foreach (var item in combinedSequence) {
                var valueIn = item.First;
                var valueOut = item.Second;

                Assert.AreEqual(Math.Round(valueIn * expectedAlignmentFactor, 4), Math.Round(valueOut, 4));
            }
        }
    }
}

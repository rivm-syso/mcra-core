using MCRA.Utils.Statistics;
using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Actions.HazardCharacterisations;
using MCRA.Simulation.Test.Mock;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Action.UncertaintyFactorial;

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
            var exposureRoutes = new List<ExposureRouteType>() {
                ExposureRouteType.Dietary,
                ExposureRouteType.Dermal,
                ExposureRouteType.Oral,
                ExposureRouteType.Inhalation
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
                    .Create(substances, effect, exposureType, 100, ExposureRouteType.Dietary, false, seed)
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad1");
            Assert.IsNotNull(data.HazardCharacterisationModels);
            Assert.IsNotNull(data.HazardCharacterisationsUnit);
            Assert.AreEqual(10, data.HazardCharacterisationModels.Count);
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
            var exposureRoutes = new List<ExposureRouteType>() {
                ExposureRouteType.Dietary,
                ExposureRouteType.Dermal,
                ExposureRouteType.Oral,
                ExposureRouteType.Inhalation
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
                    .Create(substances, null, exposureType, 100, ExposureRouteType.Dietary, true, seed)
                    .Values.Cast<HazardCharacterisation>()
                    .ToList()
            };
            var dataManager = new MockCompiledDataManager(compiledData);
            var subsetManager = new SubsetManager(dataManager, project);
            var calculator = new HazardCharacterisationsActionCalculator(project);
            TestLoadAndSummarizeNominal(calculator, data, subsetManager, "TestLoad2");
            Assert.IsNotNull(data.HazardCharacterisationModels);
            Assert.IsNotNull(data.HazardCharacterisationsUnit);
            Assert.AreEqual(10, data.HazardCharacterisationModels.Count);
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


            var exposureRoutes = new List<ExposureRouteType>() {
                ExposureRouteType.Dietary,
                ExposureRouteType.Dermal,
                ExposureRouteType.Oral,
                ExposureRouteType.Inhalation
            };
            var effect = new Effect() { Code = "effect" };
            var substances = MockSubstancesGenerator.Create(3);
            var species = "Rat";
            var data = new ActionData {
                ActiveSubstances = substances,
                SelectedEffect = effect,
                ReferenceSubstance = substances.First(),
                HazardCharacterisationsUnit = new TargetUnit(
                    SubstanceAmountUnit.Milligrams,
                    ConcentrationMassUnit.Grams,
                    TimeScaleUnit.SteadyState,
                    BiologicalMatrix.Liver
                ),
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
            var exposureRoutes = new List<ExposureRouteType>() {
                ExposureRouteType.Dietary,
                ExposureRouteType.Dermal,
                ExposureRouteType.Oral,
                ExposureRouteType.Inhalation
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
                HazardCharacterisationsUnit = new TargetUnit(
                    SubstanceAmountUnit.Milligrams,
                    ConcentrationMassUnit.Grams,
                    TimeScaleUnit.SteadyState,
                    BiologicalMatrix.Liver
                ),
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
    }
}
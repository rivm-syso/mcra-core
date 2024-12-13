using MCRA.General.Action.Settings;
using Moq;
using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.Simulation.Test.Mock.MockProject {
    /// <summary>
    /// Mocks a project's settings using the Moq framework.
    /// The internal project settings DTO is mocked here and all Settings properties
    /// This is to be able to verify invocations of settings properties during a run
    /// and summarize process
    /// </summary>
    public partial class MockProject {
        private Mock<ProjectDto> _settingsMoq;
        private Dictionary<Type, Moq.Mock> _moqsDict = [];
        //properties in this list are not added to the invocation lists
        private HashSet<string> _skipInvocationList = [
            //ProjectDto properties:
            "ProjectDto.get_ActionType",
            "ProjectDto.get_StandardActionCode",
            "ProjectDto.get_ShortOutputTemplate",
            "ProjectDto.get_ScopeKeysFilters",
            "ProjectDto.get_LoopScopingTypes",
            "ProjectDto.get_CalculationActionTypes",
            //"ProjectDto.get_",
            "ProjectDto.get_IndividualsSubsetDefinitions",
            "ProjectDto.get_LocationSubsetDefinition",
            "ProjectDto.get_PeriodSubsetDefinition",
            "ProjectDto.get_IndividualDaySubsetDefinition",
            //CovariatesSelectionSettings
            "CovariatesSelectionSettingsDto.get_NameCovariable",
            "CovariatesSelectionSettingsDto.get_NameCofactor",
            //ConcentrationModelSettings
            "ConcentrationModelSettingsDto.get_ConcentrationModelTypesPerFoodCompound",
            //UncertaintyAnalysisSettings
            "UncertaintyAnalysisSettingsDto.get_UncertaintyLowerBound",
            "UncertaintyAnalysisSettingsDto.get_UncertaintyUpperBound",
            "UncertaintyAnalysisSettingsDto.get_DoUncertaintyAnalysis",
            "UncertaintyAnalysisSettingsDto.get_NumberOfIterationsPerResampledSet",
            "UncertaintyAnalysisSettingsDto.get_NumberOfResampleCycles",
            "UncertaintyAnalysisSettingsDto.get_ReSampleConcentrations",
            "UncertaintyAnalysisSettingsDto.get_IsParametric",
            "UncertaintyAnalysisSettingsDto.get_UncertaintyType UncertaintyType",
            "UncertaintyAnalysisSettingsDto.get_ReSampleNonDietaryExposures",
            "UncertaintyAnalysisSettingsDto.get_ResampleIndividuals",
            "UncertaintyAnalysisSettingsDto.get_DoUncertaintyFactorial",
            "UncertaintyAnalysisSettingsDto.get_ReSamplePortions",
            "UncertaintyAnalysisSettingsDto.get_ReSampleProcessingFactors",
            "UncertaintyAnalysisSettingsDto.get_ReSampleAssessmentGroupMemberships",
            "UncertaintyAnalysisSettingsDto.get_ReSampleImputationExposureDistributions",
            "UncertaintyAnalysisSettingsDto.get_ReSampleRPFs",
            "UncertaintyAnalysisSettingsDto.get_ReSampleInterspecies",
            "UncertaintyAnalysisSettingsDto.get_ReSampleIntraSpecies",
            "UncertaintyAnalysisSettingsDto.get_ReSampleParameterValues",
            "UncertaintyAnalysisSettingsDto.get_ResampleKineticModelParameters",
            //OutputDetailSettings
            "OutputDetailSettingsDto.get_OutputSections",
            "OutputDetailSettingsDto.get_LowerPercentage",
            "OutputDetailSettingsDto.get_UpperPercentage",
            "OutputDetailSettingsDto.get_IsDetailedOutput",
            "OutputDetailSettingsDto.get_StoreIndividualDayIntakes",
            "OutputDetailSettingsDto.get_SelectedPercentiles",
            "OutputDetailSettingsDto.get_PercentageForDrilldown",
            "OutputDetailSettingsDto.get_PercentageForUpperTail",
            "OutputDetailSettingsDto.get_ExposureMethod",
            "OutputDetailSettingsDto.get_ExposureLevels",
            "OutputDetailSettingsDto.get_ExposureInterpretation",
            "OutputDetailSettingsDto.get_Intervals",
            "OutputDetailSettingsDto.get_ExtraPredictionLevels",
            //"OutputDetailSettingsDto.get_MaximumCumulativeRatioCutOff",
            //"OutputDetailSettingsDto.get_MaximumCumulativeRatioPercentiles",
            //"OutputDetailSettingsDto.get_MaximumCumulativeRatioMinimumPercentage",
            //MixtureSelectionSettings
            "MixtureSelectionSettingsDto.get_ExposureApproachType",
            "MixtureSelectionSettingsDto.get_TotalExposureCutOff",
            "MixtureSelectionSettingsDto.get_RatioCutOff",
            //MonteCarloSettings
            "MonteCarloSettingsDto.get_RandomSeed",
            //SubsetSettings
            "SubsetSettingsDto.get_IsPerPerson",
            "IntakeModelSettingsDto.get_NumberOfIterations",
            "HumanMonitoringSettingsDto.get_SamplingMethodCodes"
        ];


        //Returns the mocked object which should be passed to methods expecting a Project instance
        public ProjectDto Project { get; private set; }

        /// <summary>
        /// Constructor: setup of the internal mocked settings using a shadow Project
        /// instance to set all defaults of the mocked settings elements
        /// The settings are set in a partial class file generated from a T4 template
        /// which enumerates all properties ending with 'Settings' (e.g. EffectSettings)
        /// </summary>
        public MockProject(ProjectDto project = null) {
            Project = project ?? new ProjectDto();

            _settingsMoq = new Mock<ProjectDto>();
            //setup all subsettings moqs, using the method that is generated from the T4 template
            initializeSubSettingsMocks();

            //clear all invocations in the Moqs dictionary
            foreach(var m in _moqsDict.Values) {
                m.Invocations.Clear();
            }
        }

        /// <summary>
        /// Retrieve one of the mocked setting DTO's which is registered in the dictionary
        /// by type (e.g. EffectSettingsDto)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Moq.Mock GetMockedObject(Type type) {
            if (type == typeof(ProjectDto)) {
                return _settingsMoq;
            } else if (_moqsDict.TryGetValue(type, out var moq)) {
                return moq;
            }
            return null;
        }

        /// <summary>
        /// Clear all Invocations of the mocked settings properties
        /// NOTE: don't use Mock.Reset, we don't want to reset all property values
        /// </summary>
        public void ClearInvocations() {
            //clear own settings
            _settingsMoq.Invocations.Clear();
            //clear all settings mocks' settings
            foreach (var mock in _moqsDict.Values) {
                mock.Invocations.Clear();
            }
        }

        /// <summary>
        /// Clear all Invocations of the mocked settings properties
        /// NOTE: don't use Mock.Reset, we don't want to reset all property values
        /// </summary>
        public void ClearInvocations(Type type) {
            if (type == typeof(ProjectDto)) {
                _settingsMoq.Invocations.Clear();
            } else if (_moqsDict.TryGetValue(type, out var moq)) {
                moq.Invocations.Clear();
            }
        }

        /// <summary>
        /// Return all invocations on the settings objects in the form 'xxxSettings.Method'
        /// </summary>
        public HashSet<string> AllInvocations {
            get {
                return _moqsDict
                    .SelectMany(v => v.Value.Invocations, (v, i) => $"{v.Key.Name}.{i.Method.Name}")
                    .Concat(_settingsMoq.Invocations
                            .Where(i => !i.Method.Name.EndsWith("Settings"))
                            .Select(i => $"ProjectDto.{i.Method.Name}"))
                    .Where(v => !_skipInvocationList.Contains(v))
                    .ToHashSet();
            }
        }

        /// <summary>
        /// Return all invocations on the settings objects as a string
        /// of ordered values, 1 per line
        /// </summary>
        public string AllInvocationsString {
            get {
                return string.Join("\r\n", AllInvocations.Order());
            }
        }

        /// <summary>
        /// Return all invocations on the settings objects in the form 'xxxSettings.Method'
        /// </summary>
        public HashSet<string> GetInvocations(Type type) {
            IEnumerable<IInvocation> invocations;
            if (type == typeof(ProjectDto)) {
                //skip all properties ending with "Settings"
                invocations = _settingsMoq.Invocations.Where(i => !i.Method.Name.EndsWith("Settings"));
            } else if (_moqsDict.TryGetValue(type, out var moq)) {
                invocations = moq.Invocations;
            } else {
                return [];
            }
            return invocations
                    .Select(v => $"{type.Name}.{v.Method.Name}")
                    .Where(v => !_skipInvocationList.Contains(v))
                    .ToHashSet();
        }

        /// <summary>
        /// Return all invocations on the settings objects as a string
        /// of ordered values, 1 per line
        /// </summary>
        public string GetInvocationsString(Type type) {
            return string.Join("\r\n", GetInvocations(type).Order());
        }

        private void setSettings(Moq.Mock moq, object actual) {
            var type = actual.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in properties.Where(p => p.CanRead && p.CanWrite)) {
                //only set properties which don't have an [XmlIgnore] attribute
                if (prop.GetCustomAttribute(typeof(XmlIgnoreAttribute)) == null) {
                    prop.SetValue(moq.Object, prop.GetValue(actual));
                }
            }
        }
    }
}

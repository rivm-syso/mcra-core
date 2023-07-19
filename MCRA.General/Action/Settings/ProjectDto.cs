using MCRA.General.Action.Settings.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {

    #region Classes - BackwardCompatibility
    public class SelectedCompoundDto {
        public virtual string CodeCompound {
            get;
            set;
        }
    }

    /// <summary>
    /// Class to get only the deserialized version info from
    /// a project XML string
    /// </summary>
    [XmlRoot("Project")]
    public class ProjectVersionInfo {
        //structure holding version information of the MCRA version these settings
        //initialize with a new instance, containing only zeroes
        public virtual McraVersionInfo McraVersion { get; set; } = new();
    }
    #endregion

    [XmlRoot("Project")]
    public class ProjectDto {
        #region Simple Properties

        //structure holding version information of the MCRA version these settings
        //were saved with
        //initialize with a new instance, containing only zeroes
        public virtual McraVersionInfo McraVersion { get; set; } = new();

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual DateTime DateModified { get; set; }
        public virtual ActionType ActionType { get; set; } = ActionType.Unknown;
        public virtual string StandardActionCode { get; set; } = null;
        public virtual string ShortOutputTemplate { get; set; } = null;
        public virtual string DefaultTaskName { get; set; } = null;

        #endregion

        #region Collection Properties
        [XmlArrayItem("ActionType")]
        public virtual HashSet<ActionType> CalculationActionTypes { get; set; } = new();

        [XmlArrayItem("ScopeKeysFilter")]
        public virtual List<ActionScopeKeysFilterDto> ScopeKeysFilters { get; set; } = new();

        public virtual HashSet<ScopingType> LoopScopingTypes { get; set; } = new();
        public virtual List<SelectedCompoundDto> SelectedCompounds { get; set; } = new();

        public virtual List<FocalFoodDto> FocalFoods { get; set; } = new();
        [XmlArrayItem("FoodCode")]
        public virtual List<string> FoodAsEatenSubset { get; set; } = new();
        [XmlArrayItem("FoodCode")]
        public virtual List<string> ModelledFoodSubset { get; set; } = new();
        [XmlArrayItem("FoodCode")]
        public virtual List<string> SelectedScenarioAnalysisFoods { get; set; } = new();

        public virtual List<SamplesSubsetDefinitionDto> SamplesSubsetDefinitions { get; set; } = new();
        public virtual List<IndividualsSubsetDefinitionDto> IndividualsSubsetDefinitions { get; set; } = new();

        public virtual List<string> FocalFoodAsEatenSubset { get; set; } = new();
        public virtual List<string> FocalFoodAsMeasuredSubset { get; set; } = new();
        public virtual List<string> SelectedFoodSurveySubsetProperties { get; set; } = new();
        public virtual List<string> SelectedHbmSurveySubsetProperties { get; set; } = new();

        #endregion

        #region Selection Settings
        private AssessmentSettingsDto _assessmentSettings;
        private FoodSurveySettingsDto _foodSurveySettings;
        private EffectSettingsDto _effectSettings;
        private ConversionSettingsDto _conversionSettings;
        private AgriculturalUseSettingsDto _agriculturalUseSettings;
        private SubsetSettingsDto _subsetSettings;
        private LocationSubsetDefinitionDto _locationSubsetDefinition;
        private AmountModelSettingsDto _amountModelSettings;
        private ConcentrationModelSettingsDto _concentrationModelSettings;
        private CovariatesSelectionSettingsDto _covariatesSelectionSettings;
        private DietaryIntakeCalculationSettingsDto _dietaryIntakeCalculationSettings;
        private EffectModelSettingsDto _effectModelSettings;
        private FrequencyModelSettingsDto _frequencyModelSettings;
        private HumanMonitoringSettingsDto _humanMonitoringSettings;
        private IntakeModelSettingsDto _intakeModelSettings;
        private KineticModelSettingsDto _kineticModelSettings;
        private MixtureSelectionSettingsDto _mixtureSelectionSettings;
        private NonDietarySettingsDto _nonDietarySettings;
        private ScenarioAnalysisSettingsDto _scenarioAnalysisSettings;
        private ScreeningSettingsDto _screeningSettings;
        private UnitVariabilitySettingsDto _unitVariabilitySettings;
        private UncertaintyAnalysisSettingsDto _uncertaintyAnalysisSettings;
        private OutputDetailSettingsDto _outputDetailSettings;
        private MonteCarloSettingsDto _monteCarloSettings;
        private PopulationSettingsDto _populationSettings;
        private PeriodSubsetDefinitionDto _periodSubsetDefinition;
        private IndividualDaySubsetDefinitionDto _individualDaySubsetDefinition;

        public virtual AssessmentSettingsDto AssessmentSettings {
            get => _assessmentSettings ??= new();
            set => _assessmentSettings = value;
        }

        public virtual FoodSurveySettingsDto FoodSurveySettings {
            get => _foodSurveySettings ??= new();
            set => _foodSurveySettings = value;
        }

        public virtual EffectSettingsDto EffectSettings {
            get => _effectSettings ??= new();
            set => _effectSettings = value;
        }

        public virtual ConversionSettingsDto ConversionSettings {
            get => _conversionSettings ??= new();
            set => _conversionSettings = value;
        }

        public virtual AgriculturalUseSettingsDto AgriculturalUseSettings {
            get => _agriculturalUseSettings ??= new();
            set => _agriculturalUseSettings = value;
        }

        public virtual SubsetSettingsDto SubsetSettings {
            get => _subsetSettings ??= new();
            set => _subsetSettings = value;
        }

        public virtual LocationSubsetDefinitionDto LocationSubsetDefinition {
            get => _locationSubsetDefinition ??= new();
            set => _locationSubsetDefinition = value;
        }

        public virtual PeriodSubsetDefinitionDto PeriodSubsetDefinition {
            get => _periodSubsetDefinition ??= new();
            set => _periodSubsetDefinition = value;
        }

        public virtual IndividualDaySubsetDefinitionDto IndividualDaySubsetDefinition {
            get => _individualDaySubsetDefinition ??= new();
            set => _individualDaySubsetDefinition = value;
        }
        #endregion

        #region Model settings
        public virtual AmountModelSettingsDto AmountModelSettings {
            get => _amountModelSettings ??= new();
            set => _amountModelSettings = value;
        }

        public virtual ConcentrationModelSettingsDto ConcentrationModelSettings {
            get => _concentrationModelSettings ??= new();
            set => _concentrationModelSettings = value;
        }

        public virtual CovariatesSelectionSettingsDto CovariatesSelectionSettings {
            get => _covariatesSelectionSettings ??= new();
            set => _covariatesSelectionSettings = value;
        }

        public virtual DietaryIntakeCalculationSettingsDto DietaryIntakeCalculationSettings {
            get => _dietaryIntakeCalculationSettings ??= new();
            set => _dietaryIntakeCalculationSettings = value;
        }

        public virtual EffectModelSettingsDto EffectModelSettings {
            get => _effectModelSettings ??= new();
            set => _effectModelSettings = value;
        }

        public virtual FrequencyModelSettingsDto FrequencyModelSettings {
            get => _frequencyModelSettings ??= new();
            set => _frequencyModelSettings = value;
        }

        public virtual HumanMonitoringSettingsDto HumanMonitoringSettings {
            get => _humanMonitoringSettings ??= new();
            set => _humanMonitoringSettings = value;
        }

        public virtual IntakeModelSettingsDto IntakeModelSettings {
            get => _intakeModelSettings ??= new();
            set => _intakeModelSettings = value;
        }

        public virtual KineticModelSettingsDto KineticModelSettings {
            get => _kineticModelSettings ??= new();
            set => _kineticModelSettings = value;
        }

        public virtual MixtureSelectionSettingsDto MixtureSelectionSettings {
            get => _mixtureSelectionSettings ??= new();
            set => _mixtureSelectionSettings = value;
        }

        public virtual NonDietarySettingsDto NonDietarySettings {
            get => _nonDietarySettings ??= new();
            set => _nonDietarySettings = value;
        }

        public virtual ScenarioAnalysisSettingsDto ScenarioAnalysisSettings {
            get => _scenarioAnalysisSettings ??= new();
            set => _scenarioAnalysisSettings = value;
        }

        public virtual ScreeningSettingsDto ScreeningSettings {
            get => _screeningSettings ??= new();
            set => _screeningSettings = value;
        }

        public virtual UnitVariabilitySettingsDto UnitVariabilitySettings {
            get => _unitVariabilitySettings ??= new();
            set => _unitVariabilitySettings = value;
        }
        #endregion

        #region Run Settings
        public virtual UncertaintyAnalysisSettingsDto UncertaintyAnalysisSettings {
            get => _uncertaintyAnalysisSettings ??= new();
            set => _uncertaintyAnalysisSettings = value;
        }

        public virtual OutputDetailSettingsDto OutputDetailSettings {
            get => _outputDetailSettings ??= new();
            set => _outputDetailSettings = value;
        }

        public virtual MonteCarloSettingsDto MonteCarloSettings {
            get => _monteCarloSettings ??= new();
            set => _monteCarloSettings = value;
        }
        public virtual PopulationSettingsDto PopulationSettings {
            get => _populationSettings ??= new();
            set => _populationSettings = value;
        }
        #endregion

        #region Methods
        public HashSet<string> GetFilterCodes(ScopingType scopingType) =>
            ScopeKeysFilters?
                .FirstOrDefault(r => r.ScopingType == scopingType)?
                .SelectedCodes;

        /// <summary>
        /// Sets the scope keys filter for the specified scoping type.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <param name="codes"></param>
        public void SetFilterCodes(ScopingType scopingType, IEnumerable<string> codes) {
            var scopeKeysFilter = ScopeKeysFilters?.FirstOrDefault(r => r.ScopingType == scopingType);
            if (scopeKeysFilter == null) {
                scopeKeysFilter = new ActionScopeKeysFilterDto {
                    ScopingType = scopingType
                };
                ScopeKeysFilters.Add(scopeKeysFilter);
            }
            scopeKeysFilter?.SetCodesScope(codes);
        }

        public void AddCalculationAction(ActionType actionInputType) {
            CalculationActionTypes.Add(actionInputType);
        }

        public void RemoveCalculationAction(ActionType actionInputType) {
            CalculationActionTypes?.Remove(actionInputType);
        }

        [XmlIgnore]
        [JsonIgnore]
        public IDictionary<SourceTableGroup, List<IRawDataSourceVersion>> ProjectDataSourceVersions { get; set; } = null;

        /// <summary>
        /// Reconstructs a DataSourceConfiguration object from the tables in the compiled datasource
        /// </summary>
        public DataSourceConfiguration GetDataSourceConfiguration() {
            var dsConfig = new DataSourceConfiguration();
            if (ProjectDataSourceVersions != null) {
                dsConfig.DataSourceMappingRecords = ProjectDataSourceVersions
                    .SelectMany(r => r.Value, (kvp, v) => new DataSourceMappingRecord() {
                        IdRawDataSourceVersion = v.id,
                        SourceTableGroup = kvp.Key,
                        Name = v.Name,
                        RawDataSourcePath = v.DataSourcePath,
                        Checksum = v.Checksum
                    }).ToList();
            }
            return dsConfig;
        }
        #endregion
    }
}

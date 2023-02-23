using MCRA.General.Action.Settings.Dto.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace MCRA.General.Action.Settings.Dto {

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
        public virtual McraVersionInfo McraVersion { get; set; } = new McraVersionInfo();
    }
    #endregion

    [XmlRoot("Project")]
    public class ProjectDto {
        #region Simple Properties

        //structure holding version information of the MCRA version these settings
        //were saved with
        //initialize with a new instance, containing only zeroes
        public virtual McraVersionInfo McraVersion { get; set; } = new McraVersionInfo();

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
        public virtual HashSet<ActionType> CalculationActionTypes { get; set; } = new HashSet<ActionType>();

        [XmlArrayItem("ScopeKeysFilter")]
        public virtual List<ActionScopeKeysFilterDto> ScopeKeysFilters { get; set; } = new List<ActionScopeKeysFilterDto>();

        public virtual HashSet<ScopingType> LoopScopingTypes { get; set; } = new HashSet<ScopingType>();
        public virtual List<SelectedCompoundDto> SelectedCompounds { get; set; } = new List<SelectedCompoundDto>();

        public virtual List<FocalFoodDto> FocalFoods { get; set; } = new List<FocalFoodDto>();
        public virtual List<FoodAsEatenSubsetDto> FoodAsEatenSubset { get; set; } = new List<FoodAsEatenSubsetDto>();
        public virtual List<ModelledFoodSubsetDto> ModelledFoodSubset { get; set; } = new List<ModelledFoodSubsetDto>();
        public virtual List<SelectedScenarioAnalysisFoodDto> SelectedScenarioAnalysisFoods { get; set; } = new List<SelectedScenarioAnalysisFoodDto>();

        public virtual List<SamplesSubsetDefinitionDto> SamplesSubsetDefinitions { get; set; } = new List<SamplesSubsetDefinitionDto>();
        public virtual List<IndividualsSubsetDefinitionDto> IndividualsSubsetDefinitions { get; set; } = new List<IndividualsSubsetDefinitionDto>();

        public virtual List<string> FocalFoodAsEatenSubset { get; set; } = new List<string>();
        public virtual List<string> FocalFoodAsMeasuredSubset { get; set; } = new List<string>();
        public virtual List<string> SelectedFoodSurveySubsetProperties { get; set; } = new List<string>();
        public virtual List<string> SelectedHbmSurveySubsetProperties { get; set; } = new List<string>();

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
            get => _assessmentSettings ?? (_assessmentSettings = new AssessmentSettingsDto());
            set => _assessmentSettings = value;
        }

        public virtual FoodSurveySettingsDto FoodSurveySettings {
            get => _foodSurveySettings ?? (_foodSurveySettings = new FoodSurveySettingsDto());
            set => _foodSurveySettings = value;
        }

        public virtual EffectSettingsDto EffectSettings {
            get => _effectSettings ?? (_effectSettings = new EffectSettingsDto());
            set => _effectSettings = value;
        }

        public virtual ConversionSettingsDto ConversionSettings {
            get => _conversionSettings ?? (_conversionSettings = new ConversionSettingsDto());
            set => _conversionSettings = value;
        }

        public virtual AgriculturalUseSettingsDto AgriculturalUseSettings {
            get => _agriculturalUseSettings ?? (_agriculturalUseSettings = new AgriculturalUseSettingsDto());
            set => _agriculturalUseSettings = value;
        }

        public virtual SubsetSettingsDto SubsetSettings {
            get => _subsetSettings ?? (_subsetSettings = new SubsetSettingsDto());
            set => _subsetSettings = value;
        }

        public virtual LocationSubsetDefinitionDto LocationSubsetDefinition {
            get => _locationSubsetDefinition ?? (_locationSubsetDefinition = new LocationSubsetDefinitionDto());
            set => _locationSubsetDefinition = value;
        }

        public virtual PeriodSubsetDefinitionDto PeriodSubsetDefinition {
            get => _periodSubsetDefinition ?? (_periodSubsetDefinition = new PeriodSubsetDefinitionDto());
            set => _periodSubsetDefinition = value;
        }

        public virtual IndividualDaySubsetDefinitionDto IndividualDaySubsetDefinition {
            get => _individualDaySubsetDefinition ?? (_individualDaySubsetDefinition = new IndividualDaySubsetDefinitionDto());
            set => _individualDaySubsetDefinition = value;
        }
        #endregion

        #region Model settings
        public virtual AmountModelSettingsDto AmountModelSettings {
            get => _amountModelSettings ?? (_amountModelSettings = new AmountModelSettingsDto());
            set => _amountModelSettings = value;
        }

        public virtual ConcentrationModelSettingsDto ConcentrationModelSettings {
            get => _concentrationModelSettings ?? (_concentrationModelSettings = new ConcentrationModelSettingsDto());
            set => _concentrationModelSettings = value;
        }

        public virtual CovariatesSelectionSettingsDto CovariatesSelectionSettings {
            get => _covariatesSelectionSettings ?? (_covariatesSelectionSettings = new CovariatesSelectionSettingsDto());
            set => _covariatesSelectionSettings = value;
        }

        public virtual DietaryIntakeCalculationSettingsDto DietaryIntakeCalculationSettings {
            get => _dietaryIntakeCalculationSettings ?? (_dietaryIntakeCalculationSettings = new DietaryIntakeCalculationSettingsDto());
            set => _dietaryIntakeCalculationSettings = value;
        }

        public virtual EffectModelSettingsDto EffectModelSettings {
            get => _effectModelSettings ?? (_effectModelSettings = new EffectModelSettingsDto());
            set => _effectModelSettings = value;
        }

        public virtual FrequencyModelSettingsDto FrequencyModelSettings {
            get => _frequencyModelSettings ?? (_frequencyModelSettings = new FrequencyModelSettingsDto());
            set => _frequencyModelSettings = value;
        }

        public virtual HumanMonitoringSettingsDto HumanMonitoringSettings {
            get => _humanMonitoringSettings ?? (_humanMonitoringSettings = new HumanMonitoringSettingsDto());
            set => _humanMonitoringSettings = value;
        }

        public virtual IntakeModelSettingsDto IntakeModelSettings {
            get => _intakeModelSettings ?? (_intakeModelSettings = new IntakeModelSettingsDto());
            set => _intakeModelSettings = value;
        }

        public virtual KineticModelSettingsDto KineticModelSettings {
            get => _kineticModelSettings ?? (_kineticModelSettings = new KineticModelSettingsDto());
            set => _kineticModelSettings = value;
        }

        public virtual MixtureSelectionSettingsDto MixtureSelectionSettings {
            get => _mixtureSelectionSettings ?? (_mixtureSelectionSettings = new MixtureSelectionSettingsDto());
            set => _mixtureSelectionSettings = value;
        }

        public virtual NonDietarySettingsDto NonDietarySettings {
            get => _nonDietarySettings ?? (_nonDietarySettings = new NonDietarySettingsDto());
            set => _nonDietarySettings = value;
        }

        public virtual ScenarioAnalysisSettingsDto ScenarioAnalysisSettings {
            get => _scenarioAnalysisSettings ?? (_scenarioAnalysisSettings = new ScenarioAnalysisSettingsDto());
            set => _scenarioAnalysisSettings = value;
        }

        public virtual ScreeningSettingsDto ScreeningSettings {
            get => _screeningSettings ?? (_screeningSettings = new ScreeningSettingsDto());
            set => _screeningSettings = value;
        }

        public virtual UnitVariabilitySettingsDto UnitVariabilitySettings {
            get => _unitVariabilitySettings ?? (_unitVariabilitySettings = new UnitVariabilitySettingsDto());
            set => _unitVariabilitySettings = value;
        }
        #endregion

        #region Run Settings
        public virtual UncertaintyAnalysisSettingsDto UncertaintyAnalysisSettings {
            get => _uncertaintyAnalysisSettings ?? (_uncertaintyAnalysisSettings = new UncertaintyAnalysisSettingsDto());
            set => _uncertaintyAnalysisSettings = value;
        }

        public virtual OutputDetailSettingsDto OutputDetailSettings {
            get => _outputDetailSettings ?? (_outputDetailSettings = new OutputDetailSettingsDto());
            set => _outputDetailSettings = value;
        }

        public virtual MonteCarloSettingsDto MonteCarloSettings {
            get => _monteCarloSettings ?? (_monteCarloSettings = new MonteCarloSettingsDto());
            set => _monteCarloSettings = value;
        }
        public virtual PopulationSettingsDto PopulationSettings {
            get => _populationSettings ?? (_populationSettings = new PopulationSettingsDto());
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

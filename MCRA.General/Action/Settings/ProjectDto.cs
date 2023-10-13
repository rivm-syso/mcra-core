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
    public partial class ProjectDto {
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
        public virtual List<ScopeKeysFilter> ScopeKeysFilters { get; set; } = new();

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
                scopeKeysFilter = new ScopeKeysFilter {
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

using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.ScopingTypeDefinitions;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Action {

    public abstract class ActionCalculatorBase<T> : IActionCalculator
        where T : IActionResult {

        public bool ResultsComputed { get; set; } = false;
        public bool ResultsSummarized { get; set; } = false;
        public bool SimulationDataUpdated { get; set; } = false;

        private ActionSettingsSummary _actionSettingsSummary = null;

        protected ProjectDto _project;
        protected ActionType _mainActionType;
        protected ModuleConfigBase _moduleSettings;
        protected ActionModuleConfig _actionSettings;
        protected Dictionary<ActionType, ActionInputRequirement> _actionInputRequirements;
        protected Dictionary<ScopingType, ActionDataReadingRequirement> _actionDataSelectionRequirements;
        protected Dictionary<ScopingType, Dictionary<ScopingType, ActionDataLinkingRequirement>> _actionDataLinkRequirements;

        public ActionCalculatorBase(ProjectDto project) {
            _project = project;

            _mainActionType = project?.ActionType ?? ActionType.Unknown;

            //get the specific module settings from the project
            _moduleSettings = project?.GetModuleConfiguration(ActionType);
            _actionSettings = project?.ActionSettings;

            _actionInputRequirements = ModuleDefinition.Inputs
                .Union(ModuleDefinition.PrimaryEntities)
                .ToDictionary(
                    r => r,
                    r => new ActionInputRequirement() {
                        ActionType = r,
                        IsVisible = (ShouldCompute && ModuleDefinition.CalculatorInputs.Contains(r))
                            || ModuleDefinition.DataInputs.Contains(r) || ModuleDefinition.PrimaryEntities.Contains(r),
                        IsRequired = (ShouldCompute && ModuleDefinition.CalculatorInputs.Contains(r))
                            || ModuleDefinition.PrimaryEntities.Contains(r)
                    });

            if (TableGroup != SourceTableGroup.Unknown) {
                _actionDataSelectionRequirements = McraScopingTypeDefinitions.Instance
                    .TableGroupScopingTypesLookup[TableGroup]
                    .ToDictionary(r => r.Id, r => new ActionDataReadingRequirement());

                _actionDataLinkRequirements = McraScopingTypeDefinitions.Instance
                    .TableGroupScopingTypesLookup[TableGroup]
                    .ToDictionary(
                        r => r.Id,
                        r => r.ParentScopeReferences
                            .GroupBy(p => p.ReferencedScope)
                            .Select(p => new ActionDataLinkingRequirement() {
                                SourceScopingType = r.Id,
                                TargetScopingType = p.Key
                            })
                            .ToDictionary(p => p.TargetScopingType));
            }
        }

        /// <summary>
        /// Returns the module definition associated with this action calculator.
        /// </summary>
        public ModuleDefinition ModuleDefinition => McraModuleDefinitions.Instance.ModuleDefinitions[ActionType];

        /// <summary>
        /// The action type associated with this action calculator.
        /// </summary>
        public ActionType ActionType => ActionTypeAttribute.GetActionType(GetType());

        /// <summary>
        /// The table group associated with this action calculator.
        /// </summary>
        public SourceTableGroup TableGroup => ModuleDefinition.SourceTableGroup;

        /// <summary>
        /// Specifies whether the module of this action calculator is a compute module.
        /// </summary>
        public bool CanCompute => ModuleDefinition.CanCompute;

        /// <summary>
        /// Gets the inputs of the module.
        /// </summary>
        public virtual List<ActionInputRequirement> InputActionTypes => _actionInputRequirements.Values.ToList();

        /// <summary>
        /// Return whether given scoping type is a loop in the module hierarch
        /// </summary>
        /// <param name="scopingType"></param>
        /// <returns></returns>
        public virtual bool IsLoopScope(ScopingType scopingType) => _project.LoopScopingTypes?.Contains(scopingType) ?? false;

        /// <summary>
        /// Gets whether this is a compute action.
        /// </summary>
        public virtual bool ShouldCompute => (CanCompute && (_moduleSettings?.IsCompute ?? false))
                    || (CanCompute && ModuleDefinition.SourceTableGroup == SourceTableGroup.Unknown);

        /// <summary>
        /// Gets the IDs of the raw data sources of this module.
        /// </summary>
        /// <returns></returns>
        public List<int> GetRawDataSources() {
            if (_project.ProjectDataSourceVersions != null &&
               _project.ProjectDataSourceVersions.TryGetValue(TableGroup, out var versions)
            ) {
                return versions.Select(r => r.id).ToList();
            }
            return [];
        }

        public virtual IActionSettingsManager GetSettingsManager() => null;

        /// <summary>
        /// Verify the module settings.
        /// </summary>
        public void Verify() {
            var settingsManager = GetSettingsManager();
            settingsManager?.Verify(_project);
            verify();
        }

        public Dictionary<ScopingType, DataReadingReport> GetDataReadingReport(ICompiledLinkManager linkManager) {
            if (!ShouldCompute && TableGroup != SourceTableGroup.Unknown) {
                linkManager.LoadScope(TableGroup);
                var result = linkManager.GetDataReadingReports(TableGroup);

                // Set allow missing property, which may depend on action settings
                foreach (var readingRecord in result) {
                    if (readingRecord.Value.ReadingSummary != null
                        && readingRecord.Value.ReadingSummary.ScopingTypeDefinition.IsStrongEntity
                        && _actionDataSelectionRequirements.TryGetValue(readingRecord.Value.ScopingType, out var req)) {
                        var validationResult = req.Validate(readingRecord.Value.ReadingSummary);
                        readingRecord.Value.ReadingSummary.ValidationResults = validationResult;
                    }
                    if (_actionDataLinkRequirements.TryGetValue(readingRecord.Value.ScopingType, out var linkingRequirements)) {
                        foreach (var linkingRecord in readingRecord.Value.LinkingSummaries) {
                            if (linkingRequirements.TryGetValue(linkingRecord.Value.ReferencedScopingType, out var linkingRequirement)) {
                                var linkingValidationResult = linkingRequirement.Validate(linkingRecord.Value);
                                linkingRecord.Value.ValidationResults = linkingValidationResult;
                            }
                        }
                    }
                }
                return result;
            }
            return null;
        }

        public virtual bool CheckDataDependentSettings(ICompiledLinkManager linkManager) => true;

        public ActionSettingsSummary SummarizeSettings() {
            if (_actionSettingsSummary == null) {
                _actionSettingsSummary = summarizeSettings();
            }
            return _actionSettingsSummary;
        }

        public void LoadDefaultData(ActionData data) {
            if (!data.LoadedDataTypes.Contains(this.ActionType)) {
                loadDefaultData(data);
                data.LoadedDataTypes.Add(this.ActionType);
            }
        }

        /// <summary>
        /// For data modules: loads the action data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="subsetManager"></param>
        /// <param name="progressReport"></param>
        public void LoadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            if (!data.LoadedDataTypes.Contains(this.ActionType)) {
                var localProgress = progressReport.NewProgressState(1);
                var actionDisplayName = ActionType.GetDisplayName(true);
                localProgress.Update($"Loading {actionDisplayName}");
                loadData(data, subsetManager, progressReport.NewCompositeState(99));
                data.LoadedDataTypes.Add(this.ActionType);
                localProgress.Update($"Finished loading {actionDisplayName}", 100);
            }
            progressReport.MarkCompleted();
        }

        /// <summary>
        /// For calculation modules: runs the action as calculator.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="progressReport"></param>
        /// <returns></returns>
        public IActionResult Run(ActionData data, CompositeProgressState progressReport) {
            if (!ResultsComputed) {
                var localProgress = progressReport.NewProgressState(1);
                var actionDisplayName = ActionType.GetDisplayName(true);
                localProgress.Update($"Computing {actionDisplayName}");
                var result = run(data, progressReport.NewCompositeState(99));
                progressReport.MarkCompleted();
                localProgress.Update($"Finished computing {actionDisplayName}", 100);
                return result;
            }
            progressReport.MarkCompleted();
            return null;
        }

        /// <summary>
        /// Summarizes the action result.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        /// <param name="progressReport"></param>
        /// <exception cref="Exception"></exception>
        public void SummarizeActionResult(IActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            if (!ResultsSummarized && header != null) {
                var localProgress = progressReport.NewProgressState(1);
                var actionDisplayName = ActionType.GetDisplayName(true);
                localProgress.Update($"Summarizing {actionDisplayName}");
                if (actionResult is T || actionResult == null) {
                    if (ActionType == _project.ActionType) {
                        var actionMapping = ActionMappingFactory.Create(_project, _project.ActionType);
                        summarizeActionModularDesign(actionMapping, header);
                    }
                    summarizeActionResult((T)actionResult, data, header, order, progressReport.NewCompositeState(99));
                } else if (actionResult != null) {
                    throw new Exception($"Cannot summarize action result: the type of the action result does not match this action.");
                }
                ResultsSummarized = true;
                localProgress.Update($"Finished summarizing {actionDisplayName}", 100);
            }
            progressReport.MarkCompleted();
        }

        /// <summary>
        /// Summarize modular design
        /// </summary>
        private void summarizeActionModularDesign(
            ActionMapping actionMapping,
            SectionHeader header
        ) {
            if (actionMapping != null) {
                var section = new ModularDesignSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Modular design", 0);
                section.Summarize(actionMapping);
                subHeader.SaveSummarySection(section);
            }
        }

        /// <summary>
        /// Writes the output of the nominal run to the data writer.
        /// </summary>
        /// <param name="rawDataWriter"></param>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <exception cref="Exception"></exception>
        public void WriteOutputData(IRawDataWriter rawDataWriter, ActionData data, IActionResult result) {
            if (rawDataWriter != null) {
                if (result is T) {
                    writeOutputData(rawDataWriter, data, (T)result);
                } else if (result != null) {
                    throw new Exception($"Cannot update the simulation data: Type of the action result does not match this action.");
                }
            }
        }

        /// <summary>
        /// Updates the simulation data with the action result.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <exception cref="Exception"></exception>
        public void UpdateSimulationData(ActionData data, IActionResult result) {
            if (!SimulationDataUpdated) {
                if (result is T) {
                    updateSimulationData(data, (T)result);
                } else if (result != null) {
                    throw new Exception($"Cannot update the simulation data: Type of the action result does not match this action.");
                }
                SimulationDataUpdated = true;
            }
        }

        /// <summary>
        /// Gets the uncertainty/variability sources of this module.
        /// </summary>
        /// <returns></returns>
        public virtual ICollection<UncertaintySource> GetRandomSources() {
            return [];
        }

        /// <summary>
        /// Loads/bootstraps data for uncertainty run.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="factorialSet"></param>
        /// <param name="uncertaintySourceGenerators"></param>
        /// <param name="progressReport"></param>
        public void LoadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            if (!data.LoadedDataTypes.Contains(this.ActionType)) {
                loadDataUncertain(data, factorialSet, uncertaintySourceGenerators, progressReport);
                data.LoadedDataTypes.Add(this.ActionType);
            }
        }

        /// <summary>
        /// Runs the action in the context of a bootstrap/uncertainty run.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="factorialSet"></param>
        /// <param name="uncertaintySourceGenerators"></param>
        /// <param name="header"></param>
        /// <param name="progressReport"></param>
        /// <returns></returns>
        public IActionResult RunUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, SectionHeader header, CompositeProgressState progressReport) {
            if (!this.ResultsComputed) {
                var result = runUncertain(data, factorialSet, uncertaintySourceGenerators, progressReport);
                ResultsComputed = true;
                return result;
            }
            return null;
        }

        /// <summary>
        /// Summarizes action results of a bootstrap/uncertainty run.
        /// </summary>
        /// <param name="factorialSet"></param>
        /// <param name="actionResult"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="progressReport"></param>
        /// <exception cref="Exception"></exception>
        public void SummarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, IActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            if (!ResultsSummarized) {
                if (actionResult is T || actionResult == null) {
                    summarizeActionResultUncertain(factorialSet, (T)actionResult, data, header, progressReport);
                } else if (actionResult != null) {
                    throw new Exception($"Cannot summarize action result: the type of the action result does not match this action.");
                }
                ResultsSummarized = true;
            }
        }

        public void UpdateSimulationDataUncertain(ActionData data, IActionResult result) {
            if (!SimulationDataUpdated) {
                if (result is T) {
                    updateSimulationDataUncertain(data, (T)result);
                } else if (result != null) {
                    throw new Exception($"Cannot update the simulation data: Type of the action result does not match this action.");
                }
                SimulationDataUpdated = true;
            }
        }

        public void WriteOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, IActionResult result, int idBootstrap) {
            if (rawDataWriter != null) {
                if (result is T) {
                    writeOutputDataUncertain(rawDataWriter, data, (T)result, idBootstrap);
                } else if (result != null) {
                    throw new Exception($"Cannot update the simulation data: Type of the action result does not match this action.");
                }
            }
        }

        // Uncertainty factorial

        public virtual void SummarizeUncertaintyFactorial(
            UncertaintyFactorialDesign uncertaintyFactorial,
            List<UncertaintyFactorialResultRecord> factorialResult,
            SectionHeader header
        ) {
            // Default nothing
        }

        // Action comparison

        public virtual IActionComparisonData LoadActionComparisonData(ICompiledDataManager compiledDataManager, string idResultSet, string nameResultSet) {
            var result = loadActionComparisonData(compiledDataManager);
            if (result != null) {
                result.IdResultSet = idResultSet;
                result.NameResultSet = nameResultSet;
            }
            return result;
        }

        public virtual void SummarizeComparison(ICollection<IActionComparisonData> comparisonData, SectionHeader header) {
            // Default nothing
        }

        protected virtual void verify() {
        }

        protected virtual ActionSettingsSummary summarizeSettings() {
            return new ActionSettingsSummary(ActionType.GetDisplayName());
        }

        protected virtual void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            // Default nothing
        }

        protected virtual void loadDefaultData(ActionData data) {
            // Default nothing
        }

        protected virtual T run(ActionData data, CompositeProgressState progressReport) {
            return default;
        }

        protected virtual void summarizeActionResult(T actionResult, ActionData data, SectionHeader header, int sectionOrder, CompositeProgressState progressReport) {
            // Default nothing
        }

        protected virtual void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, T result) {
            // Default nothing
        }

        protected virtual void updateSimulationData(ActionData data, T result) {
            // Default nothing
        }

        protected virtual void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            // Default nothing
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update(100);
        }

        protected virtual T runUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            return default;
        }

        protected virtual void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, T actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            // Default nothing
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update(100);
        }

        protected virtual void writeOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, T result, int idBootstrap) {
            // Default nothing
        }

        protected virtual void updateSimulationDataUncertain(ActionData data, T result) {
            updateSimulationData(data, result);
        }

        protected virtual IActionComparisonData loadActionComparisonData(ICompiledDataManager compiledDataManager) {
            // Default return null
            return null;
        }

        public IRandom GetRandomGenerator(int seed) {
            return (seed == 0) ? new McraRandomGenerator() : new McraRandomGenerator(seed);
        }
    }
}

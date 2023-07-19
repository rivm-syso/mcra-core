using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.General.ScopingTypeDefinitions;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Action {

    public abstract class ActionSettingsSummarizerBase : IActionSettingsSummarizer {

        public abstract ActionType ActionType { get; }

        public abstract ActionSettingsSummary Summarize(ProjectDto project);

        public ModuleDefinition ModuleDefinition {
            get {
                return McraModuleDefinitions.Instance.ModuleDefinitions[ActionType];
            }
        }

        protected void summarizeDataSources(
            ProjectDto project,
            ActionSettingsSummary section,
            bool skipDataSource = true
        ) {
            if(project.ProjectDataSourceVersions != null &&
               project.ProjectDataSourceVersions
                .TryGetValue(ModuleDefinition.SourceTableGroup, out var rds)
            ) {
                if (!skipDataSource) {
                    foreach (var item in rds) {
                        section.SummarizeDataSource(ModuleDefinition.SourceTableGroup, item);
                    }
                }
                summarizeScopeFilters(project, section);
            }
        }

        protected void summarizeScopeFilters(ProjectDto project, ActionSettingsSummary section) {
            var scopingTypes = McraScopingTypeDefinitions.Instance.GetTableGroupUserSelectionTypes(ModuleDefinition.SourceTableGroup);
            if (scopingTypes?.Any() ?? false) {
                foreach (var scopingType in scopingTypes) {
                    var filterCodes = project.GetFilterCodes(scopingType.Id);
                    if (filterCodes?.Any() ?? false) {
                        section.SummarizeSetting(
                            $"Selected codes {scopingType.Name.ToLowerInvariant()}",
                            string.Join(", ", filterCodes)
                        );
                    }
                }
            }
        }

        protected void summarizeDataOrCompute(ProjectDto project, ActionSettingsSummary section) {
            var description = project.CalculationActionTypes.Contains(ActionType) ? $"{ActionType.GetDisplayName()} are calculated" : $"{ActionType.GetDisplayName()} are data";
            section.SummarizeSetting(description, string.Empty);
        }
    }
}

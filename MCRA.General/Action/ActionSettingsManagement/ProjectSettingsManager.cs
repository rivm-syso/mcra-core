using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.General.ScopingTypeDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public sealed class ProjectSettingsManager {

        public ProjectSettingsManager() {
        }

        /// <summary>
        /// Sets whether the data of the specified action type should be computed or obtained from data.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="actionType"></param>
        /// <param name="isCompute"></param>
        public void SetIsCompute(
            ProjectDto project,
            ActionType actionType,
            bool isCompute
        ) {
            if (isCompute) {
                project.AddCalculationAction(actionType);
                var module = McraModuleDefinitions.Instance.ModuleDefinitions[actionType];
                if (module.SourceTableGroup != SourceTableGroup.Unknown) {
                    var moduleScopingTypes = McraScopingTypeDefinitions.Instance
                        .TableGroupScopingTypesLookup[module.SourceTableGroup]?
                        .Select(r => r.Id)
                        .ToHashSet();
                    if (moduleScopingTypes?.Count > 0) {
                        project.LoopScopingTypes.RemoveWhere(r => moduleScopingTypes.Contains(r));
                    }
                }
            } else {
                project.RemoveCalculationAction(actionType);
            }
        }

        /// <summary>
        /// Gets the scope codes of the specified table.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="scopingType"></param>
        public HashSet<string> GetFilterCodes(ProjectDto project, ScopingType scopingType) {
            var result = project?.GetFilterCodes(scopingType);
            return result;
        }

        /// <summary>
        /// Extends the scope of the target table with the provided codes.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="scopingType"></param>
        /// <param name="codes"></param>
        public void ExtendScope(ProjectDto project, ScopingType scopingType, string[] codes) {
            var codesInScope = project.GetFilterCodes(scopingType);
            var newScope = codesInScope.Union(codes);
            project.SetFilterCodes(scopingType, newScope);
        }

        /// <summary>
        /// Reduces the scope. Removes the provided codes from the scope.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="scopingType"></param>
        /// <param name="codes"></param>
        public void ReduceScope(ProjectDto project, ScopingType scopingType, string[] codes) {
            var codesInScope = GetFilterCodes(project, scopingType);
            if (!codesInScope.Any()) {
                project.SetFilterCodes(scopingType, codes);
            } else {
                var newScope = codesInScope.Where(r => !codes.Contains(r));
                project.SetFilterCodes(scopingType, newScope);
            }
        }
    }
}

using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.Simulation.Actions;

namespace MCRA.Simulation.Action {
    public class ActionMappingFactory {

        public static ActionMapping Create(ProjectDto project, ActionType actionType) {
            var mapping = new ActionMapping {
                MainActionType = actionType,
                Project = project,
                ModuleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[actionType],
                ModuleMappingsDictionary = [],
                AvailableUncertaintySources = [],
                OutputSettings = []
            };
            int order = 0;
            var calculatorProvider = new ActionCalculatorProvider();
            createRecursive(mapping, project, actionType, null, true, true, ref order, calculatorProvider);
            collectModuleVisibilities(mapping);
            order = 0;
            updateOrderRecursive(mapping, actionType, ref order);
            collectFilters(mapping);
            checkSettings(mapping);
            return mapping;
        }

        private static void createRecursive(
            ActionMapping mapping,
            ProjectDto project,
            ActionType actionType,
            ActionType? parent,
            bool required,
            bool visible,
            ref int order,
            ActionCalculatorProvider actionCalculatorProvider
        ){
            if (!mapping.ModuleMappingsDictionary.TryGetValue(actionType, out ActionModuleMapping moduleMapping)) {
                var moduleDefinition = McraModuleDefinitions.Instance.ModuleDefinitions[actionType];
                var calculator = actionCalculatorProvider.Get(actionType, project, true);
                moduleMapping = new ActionModuleMapping() {
                    ActionType = actionType,
                    ModuleDefinition = moduleDefinition,
                    IsMainModule = parent == null,
                    ActionCalculator = calculator,
                    IsRequired = required,
                    IsVisible = visible,
                    IsCompute = calculator.ShouldCompute,
                    RawDataSources = calculator.GetRawDataSources(),
                    InputRequirements = calculator.InputActionTypes,
                    TableGroup = moduleDefinition.SourceTableGroup
                };
                mapping.ModuleMappingsDictionary.Add(actionType, moduleMapping);
                var inputs = moduleMapping.InputRequirements.OrderByDescending(r => r.IsVisible).ToList();
                foreach (var input in inputs) {
                    createRecursive(mapping, project, input.ActionType, actionType,
                                    required && input.IsRequired, visible && input.IsVisible,
                                    ref order, actionCalculatorProvider);
                }
                moduleMapping.Order = order++;
            } else {
                moduleMapping.IsRequired |= required;
                moduleMapping.IsVisible |= visible;
            }

            if (parent.HasValue) {
                moduleMapping.UsedByModules.Add(parent.Value);
            }
        }

        public static void collectModuleVisibilities(ActionMapping actionMapping) {
            var moduleMappings = actionMapping.GetModuleMappings().OrderByDescending(r => r.Order).ToList();
            foreach (var moduleMapping in moduleMappings) {
                var isRequired = moduleMapping.IsMainModule;
                var isVisible = moduleMapping.IsMainModule;
                foreach (var userModule in moduleMapping.UsedByModules) {
                    if (!isRequired || !isVisible) {
                        var usedByModule = actionMapping.ModuleMappingsDictionary[userModule];
                        var userRequirement = usedByModule.InputRequirements
                            .First(r => r.ActionType == moduleMapping.ActionType);
                        isRequired |= (usedByModule.IsRequired || usedByModule.IsSpecified) && userRequirement.IsRequired;
                        isVisible |= (usedByModule.IsRequired || usedByModule.IsSpecified) && userRequirement.IsVisible;
                    }
                }
                moduleMapping.IsRequired = isRequired;
                moduleMapping.IsVisible = isVisible;
            }
        }

        public static void updateOrderRecursive(
            ActionMapping actionMapping,
            ActionType actionType,
            ref int order,
            HashSet<ActionType> doneList = null
        ) {
            if(doneList == null) {
                doneList = [];
            }
            if (!doneList.Contains(actionType)) {
                var currentMapping = actionMapping.ModuleMappingsDictionary[actionType];
                doneList.Add(actionType);

                // Process visible module inputs
                var inputs = currentMapping.InputRequirements.Where(r => r.IsVisible).ToList();
                foreach (var inputRequirement in inputs) {
                    updateOrderRecursive(actionMapping, inputRequirement.ActionType, ref order, doneList);
                }

                // Process module itself
                currentMapping.Order = order++;

                // Process visible module inputs
                inputs = currentMapping.InputRequirements.Where(r => !r.IsVisible).ToList();
                foreach (var inputRequirement in inputs) {
                    updateOrderRecursive(actionMapping, inputRequirement.ActionType, ref order, doneList);
                }
            }
        }

        private static void collectFilters(ActionMapping actionMapping) {
            foreach (var moduleMapping in actionMapping.GetModuleMappings()) {
                if (moduleMapping.IsRequired || moduleMapping.IsSpecified) {
                    foreach (var setting in moduleMapping.ModuleDefinition.UncertaintySettingsItems) {
                        actionMapping.AvailableUncertaintySources.Add(setting);
                    }
                    foreach (var setting in moduleMapping.ModuleDefinition.OutputSettingsItems) {
                        if (!actionMapping.OutputSettings.Contains(setting)) {
                            actionMapping.OutputSettings.Add(setting);
                        }
                    }
                }
            }
        }

        private static void checkSettings(ActionMapping actionMapping) {
            var mappings = actionMapping.GetModuleMappings();
            foreach (var moduleMapping in mappings) {
                var moduleSettings = moduleMapping.ActionCalculator.SummarizeSettings();

                var activeInputMappings = moduleMapping.InputRequirements
                    .Where(r => r.IsVisible && (r.IsRequired || actionMapping.ModuleMappingsDictionary[r.ActionType].IsSpecified))
                    .ToList();

                var activeScopes = activeInputMappings
                    .Where(r => McraModuleDefinitions.Instance.ModuleDefinitions[r.ActionType].ModuleType == ModuleType.PrimaryEntityModule)
                    .ToList();

                var activeInputs = activeInputMappings
                    .Where(r => McraModuleDefinitions.Instance.ModuleDefinitions[r.ActionType].ModuleType != ModuleType.PrimaryEntityModule)
                    .ToList();

                moduleSettings.ScopeSubSections = activeScopes
                    .Select(r => actionMapping.ModuleMappingsDictionary[r.ActionType].Settings)
                    .Where(r => r != null)
                    .ToList();
                moduleSettings.SubActionSubSections = activeInputs
                    .Select(r => actionMapping.ModuleMappingsDictionary[r.ActionType].Settings)
                    .Where(r => r != null)
                    .ToList();
                moduleSettings.IsActionRoot = moduleMapping.ActionType == actionMapping.MainActionType;
                moduleMapping.Settings = moduleSettings;
                moduleMapping.IsSettingsValid = moduleMapping.IsSpecified
                    && (moduleMapping.Settings?.IsValid ?? true)
                    && activeInputMappings.All(r => actionMapping.ModuleMappingsDictionary[r.ActionType].IsSettingsValid);
            }
        }
    }
}

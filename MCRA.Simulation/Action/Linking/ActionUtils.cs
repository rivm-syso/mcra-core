using MCRA.General;
using MCRA.General.ModuleDefinitions;

namespace MCRA.Simulation.Action.Linking {
    public static class ActionUtils {
        /// <summary>
        /// Find all inputs (data, calculators and entities). Set also the ModuleType for each input.
        /// Only select those inputs which are required (allInputModules);
        /// </summary>
        /// <param name="allInputModules"></param>
        /// <returns></returns>
        public static ICollection<(ActionType, ModuleType, List<string>)> GetAllRelations(List<string> allInputModules) {
            var allModuleDefinitions = McraModuleDefinitions.Instance.ModuleDefinitions.ToDictionary(c => c.Key.ToString(), c => c.Value);
            var moduleDefinitions = allInputModules.Select(c => allModuleDefinitions[c]).ToList();

            //Select all relations
            var allRelations = new List<(ActionType, ModuleType, List<string>)>();
            foreach (var module in moduleDefinitions) {
                var result = new List<string>();
                var moduleType = ModuleType.SupportModule;
                if (module.ModuleType == ModuleType.PrimaryEntityModule) {
                    moduleType = ModuleType.PrimaryEntityModule;
                } else if (module.ModuleType == ModuleType.DataModule) {
                    moduleType = ModuleType.DataModule;
                } else if (module.ModuleType == ModuleType.CalculatorModule && module.TableGroup == null) {
                    moduleType = ModuleType.CalculatorModule;
                };
                result.AddRange(module.SelectionInputs.Where(c => allInputModules.Contains(c)));
                result.AddRange(module.CalculatorInputs.Where(c => allInputModules.Contains(c.ToString())).Select(c => c.ToString()).ToList());
                result.AddRange(module.Entities.Where(c => allInputModules.Contains(c)));
                allRelations.Add((module.ActionType, moduleType, result));
            }
            return allRelations;
        }

    }
}

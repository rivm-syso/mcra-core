using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.Linking;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleDiagramCreator.DiagramCreators;

namespace ModuleDiagramCreator.Test.IntegrationTests {

    [TestClass]
    public class DiagramCreatorTests {
        private Dictionary<string, ModuleDefinition> _allModuleDefinitions;

        [TestMethod]
        [TestCategory("Sandbox Tests")]
        [DataRow(ActionType.Risks, true)]
        [DataRow(ActionType.Consumptions, true)]
        [DataRow(ActionType.Concentrations, true)]
        [DataRow(ActionType.HumanMonitoringAnalysis, true)]
        [DataRow(ActionType.DietaryExposures, true)]
        [DataRow(ActionType.Risks, false)]
        [DataRow(ActionType.Consumptions, false)]
        [DataRow(ActionType.Concentrations, false)]
        [DataRow(ActionType.HumanMonitoringAnalysis, false)]
        [DataRow(ActionType.DietaryExposures, false)]
        public void PlantUmlDiagramCreator_TestCreate(
            ActionType actionType,
            bool isBasedOnMappings
        ) {
            var moduleDefinitions = McraModuleDefinitions.Instance.ModuleDefinitions;
            _allModuleDefinitions = moduleDefinitions.ToDictionary(c => c.Key.ToString(), c => c.Value);
            var actions = new List<string>() { actionType.ToString() };
            var outputPath = TestUtilities.GetOrCreateTestOutputPath("CreateSVG");

            //Get all inputs for the specified module or modules
            //keep this for the moment
            var allInputModules = _getAllModuleActionsRecursive(actions, []);
            if (isBasedOnMappings) {
                //Get all mappings
                var project = new ProjectDto() { ActionType = actionType };
                var mapping = ActionMappingFactory.Create(project, actionType);
                allInputModules = mapping.GetModuleMappings()
                    .Where(c => c.IsRequired)
                    .Select(c => c.ActionType.ToString())
                    .ToList();
            }

            //Select all input module definitions
            var allRelations = ActionUtils.GetAllRelations(allInputModules);

            var diagramCreator = new PlantUmlDiagramCreator();
            var options = new CreateOptions() {
                NodeSep = 10,
                RankSep = 10,
                LineWrap = 2
            };

            diagramCreator.CreateToFile(
                options,
                isBasedOnMappings ? $"{actionType.ToString()}_mappings" : actionType.ToString(),
                outputPath,
                allRelations,
                actionType.ToString()
            );
        }

        [TestMethod]
        [DataRow(ActionType.Risks, true)]
        [DataRow(ActionType.Consumptions, true)]
        [DataRow(ActionType.Concentrations, true)]
        [DataRow(ActionType.HumanMonitoringAnalysis, true)]
        [DataRow(ActionType.DietaryExposures, true)]
        [DataRow(ActionType.Risks, false)]
        [DataRow(ActionType.Consumptions, false)]
        [DataRow(ActionType.Concentrations, false)]
        [DataRow(ActionType.HumanMonitoringAnalysis, false)]
        [DataRow(ActionType.DietaryExposures, false)]
        public void GraphvizDiagramCreator_Mappings_TestCreate(
            ActionType actionType,
            bool isBasedOnMappings
        ) {
            var moduleDefinitions = McraModuleDefinitions.Instance.ModuleDefinitions;
            _allModuleDefinitions = moduleDefinitions.ToDictionary(c => c.Key.ToString(), c => c.Value);
            var actions = new List<string>() { actionType.ToString() };
            var outputPath = TestUtilities.GetOrCreateTestOutputPath("CreateSVG");

            //Get all inputs for the specified module or modules
            //keep this for the moment
            var allInputModules = _getAllModuleActionsRecursive(actions, []);

            //Prune based on mappings
            if (isBasedOnMappings) {
                //Get all mappings and select the actions that required
                var project = new ProjectDto() { ActionType = actionType };
                var mapping = ActionMappingFactory.Create(project, actionType);
                allInputModules = mapping.GetModuleMappings()
                    .Where(c => c.IsRequired)
                    .Select(c => c.ActionType.ToString())
                    .ToList();
            }

            //Select all input module definitions
            var allRelations = ActionUtils.GetAllRelations(allInputModules);

            var diagramCreator = new GraphvizDiagramCreator();
            var options = new CreateOptions() {
                OutputDotFile = true,
                LayoutAlgorithm = "dot",
                OutputFormat = CreateOptions._defaultOutputFormat,
                LineWrap = 2
            };

            diagramCreator.CreateToFile(
                options,
                isBasedOnMappings ? $"{actionType.ToString()}_mappings" : actionType.ToString(),
                outputPath,
                allRelations,
                actionType.ToString()
            );
        }

        /// <summary>
        /// Find all inputs recursively
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="allInputActions"></param>
        /// <returns></returns>
        private List<string> _getAllModuleActionsRecursive(List<string> inputs, List<string> allInputActions) {
            foreach (var input in inputs) {
                if (!allInputActions.Contains(input)) {
                    allInputActions.Add(input);
                    var specifiedModule = _allModuleDefinitions.Single(c => c.Key == input).Value;
                    var foundActions = specifiedModule.CalculatorInputs.Select(c => c.ToString()).ToList();
                    foundActions.AddRange(specifiedModule.SelectionInputs.ToList());
                    foundActions.AddRange(specifiedModule.Entities.ToList());
                    if (foundActions.Any()) {
                        _getAllModuleActionsRecursive(foundActions, allInputActions);
                    }
                }
            }
            return allInputActions;
        }
    }
}

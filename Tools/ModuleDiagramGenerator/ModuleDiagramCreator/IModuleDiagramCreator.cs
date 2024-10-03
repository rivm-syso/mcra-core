using MCRA.General;

namespace ModuleDiagramCreator {
    public interface IModuleDiagramCreator {
        public void CreateToFile(
            CreateOptions options,
            string diagramFilename,
            string outputDir,
            ICollection<(ActionType actionType, ModuleType moduleType, List<string> inputs)> relationships,
            string actionType = null
        );

        public int CreateToFile(
            CreateOptions options
        );
    }
}

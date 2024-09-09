using MCRA.General;

namespace ModuleDiagramCreator {
    public interface IModuleDiagramCreator {
        public void CreateToFile(
            CreateOptions options,
            string diagramFilename,
            string outputDir,
            Dictionary<(ActionType, ModuleType), List<string>> relationships,
            string actionType = null
        );

        public int CreateToFile(
            CreateOptions options
        );
    }
}

using MCRA.General;
namespace ModuleDiagramCreator.DiagramCreators {

    public class ModuleDiagramCreator {

        /// <summary>
        /// Note, for PlantUml the syntax = 'gold-yellowgreen'.
        /// Note, for Graphviz the syntax = 'gold:yellowgreen'.
        /// </summary>
        public readonly string EntityColor = "lavender";
        public readonly string DataColor = "yellowgreen";
        public readonly string CalculatorColor = "gold";
        public readonly string DataAndCalculatorColor = "gold:yellowgreen";
        public readonly string _titleColor = "lightblue";

        public string GetColors(ModuleType moduleType) {
            var color = string.Empty;
            if (moduleType == ModuleType.PrimaryEntityModule) {
                color = EntityColor;
            } else if (moduleType == ModuleType.DataModule) {
                color = DataColor;
            } else if (moduleType == ModuleType.CalculatorModule) {
                color = CalculatorColor;
            } else {
                color = DataAndCalculatorColor;
            }
            return color;
        }
    }
}

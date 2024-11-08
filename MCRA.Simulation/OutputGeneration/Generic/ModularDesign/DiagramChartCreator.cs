using System.Reflection;
using MCRA.Utils.ExtensionMethods;
using ModuleDiagramCreator;
using ModuleDiagramCreator.DiagramCreators;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DiagramChartCreator : IReportChartCreator {

        private readonly ModularDesignSection _section;

        public int Width { get; } = 15;
        public int Height { get; } = 10;
        public string ChartId {
            get {
                var pictureId = "de85afff-2fbd-497b-a0e8-c03773cb8f9e";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public string Title => string.Empty;

        public DiagramChartCreator(ModularDesignSection section) {
            _section = section;
        }

        public void CreateToSvg(string fileName) {
            Create(fileName);
        }

        public void CreateToPng(string filename) {
            throw new NotImplementedException();
        }

        public string ToSvgString(int width, int height) {
            throw new NotImplementedException();
        }

        public void WritePngToStream(Stream stream) {
            throw new NotImplementedException();
        }

        private void Create(
            string fileName
        ) {
            var diagramCreator = new GraphvizDiagramCreator();
            diagramCreator.GraphVizBinariesDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "graphviz");
            var options = new CreateOptions() {
                OutputDotFile = true,
                LayoutAlgorithm = "dot",
                OutputFormat = CreateOptions._defaultOutputFormat,
                LineWrap = 2,
                Height = Height,
                Width = Width,
            };
            diagramCreator.CreateToFile(
                options,
                fileName,
                Path.GetDirectoryName(fileName),
                _section.AllRelations,
                _section.ActionType.ToString()
            );
        }
    }
}

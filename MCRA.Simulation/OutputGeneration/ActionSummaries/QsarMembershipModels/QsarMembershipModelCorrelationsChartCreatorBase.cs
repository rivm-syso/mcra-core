using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class QsarMembershipModelCorrelationsChartCreatorBase : OxyPlotCorrelationsChartCreator {

        private const int _cellSize = 20;

        protected QsarMembershipModelCorrelationsSection _section;

        public QsarMembershipModelCorrelationsChartCreatorBase(QsarMembershipModelCorrelationsSection section) {
            _section = section;
            Height = 200 + Math.Max(_section.ModelNames.Count * _cellSize, 100);
            Width = 200 + Math.Max(_section.ModelNames.Count * _cellSize, 100);
        }

        protected static PlotModel create(double[,] correlations, List<string> modelNames, string title) {
            var plotModel = createScatterHeatmap(correlations, modelNames, modelNames, _cellSize);
            plotModel.Title = title;
            return plotModel;
        }
    }
}

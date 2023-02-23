using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class MolecularDockingModelCorrelationsChartCreatorBase : OxyPlotCorrelationsChartCreator {

        private const int _cellSize = 20;

        protected MolecularDockingModelCorrelationsSummarySection _section;

        public MolecularDockingModelCorrelationsChartCreatorBase(MolecularDockingModelCorrelationsSummarySection section) {
            _section = section;
            Height = 200 + _section.ModelNames.Count * _cellSize;
            Width = 200 + _section.ModelNames.Count * _cellSize;
        }

        protected static PlotModel create(double[,] correlations, List<string> modelNames) {
            var plotModel = createScatterHeatmap(correlations, modelNames, modelNames, _cellSize);
            return plotModel;
        }
    }
}

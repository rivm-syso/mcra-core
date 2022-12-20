using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using System;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ActiveSubstanceModelCorrelationsChartCreatorBase : OxyPlotCorrelationsChartCreator {

        private const int _cellSize = 25;

        protected ActiveSubstanceModelCorrelationsSection _section;

        public ActiveSubstanceModelCorrelationsChartCreatorBase(ActiveSubstanceModelCorrelationsSection section) {
            _section = section;
            Height = 200 + Math.Max(_section.ModelNames.Count * _cellSize, 100);
            Width = 200 + Math.Max(_section.ModelNames.Count * _cellSize, 100);
        }

        protected static PlotModel create(double[,] correlations, List<string> modelNames) {
            var plotModel = createScatterHeatmap(correlations, modelNames, modelNames, _cellSize);
            return plotModel;
        }
    }
}

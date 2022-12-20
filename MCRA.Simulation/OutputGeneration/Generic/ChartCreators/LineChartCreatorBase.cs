using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class LineChartCreatorBase : OxyPlotLineCreator {

        protected virtual PlotModel createPlotModel(List<int> xValues, List<double> yValues, string title, string xtitle, string ytitle) {
            var plotModel = createDefaultPlotModel(title);
            return plotModel;
        }

        protected virtual PlotModel createPlotModel(List<double> xValues, List<double> yValues, string title, string xtitle, string ytitle) {
            var plotModel = createDefaultPlotModel(title);
            return plotModel;
        }

        protected virtual PlotModel createPlotModel(KineticModelSection section, string title, string xtitle, string ytitle) {
            var plotModel = createDefaultPlotModel(title);
            return plotModel;
        }

        protected virtual PlotModel createPlotModel(List<int> xValues, List<double> yValues, string title, string xtitle, string ytitle, TimeUnit timeunit) {
            var plotModel = createDefaultPlotModel(title);
            return plotModel;
        }
    }
}

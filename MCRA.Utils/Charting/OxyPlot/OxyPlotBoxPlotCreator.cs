﻿using OxyPlot.Axes;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotBoxPlotCreator : OxyPlotChartCreator {


        protected LinearAxis createLinearBottomAxis(string title) {
            return new LinearAxis() {
                Position = AxisPosition.Bottom,
                Title = title,
            };
        }

        protected LinearAxis createLinearLeftAxis(string title) {
            return new LinearAxis() {
                Position = AxisPosition.Left,
                Title = title,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
                MajorStep = 100,
                MinorStep = 100, 
            };
        }
        protected LogarithmicAxis createLogarithmicLeftAxis(string title) {
            return new LogarithmicAxis() {
                Position = AxisPosition.Left,
                Title = title,
                MaximumPadding = 0.1,
                MinimumPadding = 0.1,
                MajorStep = 100,
                MinorStep = 100,
            };
        }

    }
}

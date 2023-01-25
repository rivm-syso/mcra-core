using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class DriverCompoundsChartCreatorBase : OxyPlotChartCreator {

        protected PlotModel createPlotModel(string title) {
            var plotModel = new PlotModel() {
                Title = title,
                TitleFontWeight = FontWeights.Normal,
                TitleFontSize = 12,
                IsLegendVisible = true,
                ClipTitle = false
            };

            var legend = new Legend {
                LegendBackground = OxyColor.FromArgb(200, 255, 255, 255),
                LegendBorder = OxyColors.Undefined,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomLeft,
                LegendFontSize = 10,
            };
            plotModel.Legends.Add(legend);

            return plotModel;
        }

        protected LineSeries createLineSeries(OxyColor color) {
            return new LineSeries() {
                Color = color,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1,
            };
        }

        protected LineAnnotation createLineAnnotation(double y, string text) {
            return new LineAnnotation() {
                Type = LineAnnotationType.Horizontal,
                Color = OxyColors.Undefined,
                Y = y,
                Text = text,
                FontSize = 10,
            };
        }
    }
}

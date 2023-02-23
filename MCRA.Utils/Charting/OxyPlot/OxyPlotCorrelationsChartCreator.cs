using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Utils.Charting.OxyPlot {
    public abstract class OxyPlotCorrelationsChartCreator : OxyPlotHeatMapCreator {

        protected static PlotModel createHeatmap(double[,] matrix, List<string> namesAxisX, List<string> namesAxisY, int labelLengthLimit = 15) {
            var plotModel = new PlotModel() {
                TitleFontWeight = FontWeights.Normal,
                TitleFontSize = 12,
                IsLegendVisible = false,
            };

            var heatMapSeries = new HeatMapSeries() {
                Interpolate = false,
                X0 = 0,
                X1 = matrix.GetLength(0) - 1,
                Y0 = matrix.GetLength(1) - 1,
                Y1 = 0,
                LabelFontSize = 0,
                Data = matrix,
            };

            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1);

            var colorAxis = new LinearColorAxis {
                Palette = OxyPalettes.BlueWhiteRed(200).Reverse(),
                Position = AxisPosition.Right,
                IsAxisVisible = false,
            };
            plotModel.Axes.Add(colorAxis);

            var horizontalAxis = new CategoryAxis() {
                Angle = 90,
                MinorStep = 1,
                FontSize = 9,
                Position = AxisPosition.Top,
                Minimum = -.5,
                Maximum = namesAxisX.Count - .5,
            };
            foreach (var name in namesAxisX) {
                var shortname = name.LimitTo(labelLengthLimit);
                horizontalAxis.Labels.Add(shortname);
            }
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new CategoryAxis() {
                MinorStep = 1,
                FontSize = 9,
                Position = AxisPosition.Left,
                Minimum = -.5,
                Maximum = namesAxisY.Count - .5,
            };

            var namesAxisYReverse = namesAxisY;
            namesAxisYReverse.Reverse();
            foreach (var name in namesAxisY) {
                var shortname = name.LimitTo(labelLengthLimit);
                verticalAxis.Labels.Add(shortname);
            }
            plotModel.Axes.Add(verticalAxis);

            plotModel.Series.Add(heatMapSeries);
            return plotModel;
        }

        protected static PlotModel createScatterHeatmap(double[,] matrix, List<string> namesAxisX, List<string> namesAxisY, int cellSize = 15, int labelLengthLimit = 13) {
            var plotModel = new PlotModel {
                TitleFontWeight = FontWeights.Normal,
                TitleFontSize = 12,
                IsLegendVisible = false,
                PlotAreaBorderColor = OxyColors.Transparent
            };

            var colorAxis = new LinearColorAxis {
                Palette = OxyPalettes.BlueWhiteRed(200).Reverse(),
                Position = AxisPosition.Left,
                //IsAxisVisible = false,
                Minimum = -1D,
                Maximum = 1D,
            };
            plotModel.Axes.Add(colorAxis);

            var horizontalAxis = new CategoryAxis() {
                Angle = 45,
                MinorStep = 1,
                Position = AxisPosition.Top,
                Minimum = -.5,
                Maximum = namesAxisX.Count - .5,
                AxislineStyle = LineStyle.Solid,
            };
            foreach (var name in namesAxisX) {
                var shortname = name.LimitTo(labelLengthLimit);
                horizontalAxis.Labels.Add(shortname);
            }
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new CategoryAxis() {
                MinorStep = 1,
                Position = AxisPosition.Right,
                Minimum = -.5,
                Maximum = namesAxisY.Count - .5,
                AxislineStyle = LineStyle.Solid,
            };

            var namesAxisYReverse = namesAxisY;
            namesAxisYReverse.Reverse();
            foreach (var name in namesAxisY) {
                var shortname = name.LimitTo(labelLengthLimit);
                verticalAxis.Labels.Add(shortname);
            }
            plotModel.Axes.Add(verticalAxis);

            plotModel.Axes.Add(new LinearAxis() {
                Position = AxisPosition.Left,
                IsAxisVisible = false
            });

            plotModel.Axes.Add(new LinearAxis() {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false   
            });

            var top = true;
            var scatterSeries = new ScatterSeries() {
                MarkerSize = 1,
                MarkerType = MarkerType.Circle,
            };
            var points = new List<ScatterPoint>();
            var dimX = matrix.GetUpperBound(0) + 1;
            var dimY = matrix.GetUpperBound(1) + 1;
            for (int j = 0; j < dimY; j++) {
                for (int i = 0; i < dimX; i++) {
                    if (!top || i >= (dimY - 1) - j) {
                        var value = matrix[i, (dimY - 1) - j];
                        points.Add(new ScatterPoint(i, j, 1 + .5 * (cellSize - 2) * Math.Abs(value), value));
                    }
                }
            }
            scatterSeries.ItemsSource = points;
            plotModel.Series.Add(scatterSeries);

            return plotModel;
        }
    }
}

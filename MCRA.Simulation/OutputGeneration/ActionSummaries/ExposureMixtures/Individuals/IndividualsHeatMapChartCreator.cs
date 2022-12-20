using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualsHeatMapChartCreator : OxyPlotChartCreator {

        private List<IndividualComponentRecord> _components;
        private int _numberOfindividuals;
        private string _sectionId;
        private int _clusterId;
        private int _numberOfClusters;
        private int _numberOfComponents;
        public IndividualsHeatMapChartCreator(
                string sectionId,
                List<IndividualComponentRecord> components,
                int numberOfindividual,
                int numberOfComponents,
                int numberOfClusters,
                int clusterId = 0
            ) {
            _components = components;
            _sectionId = sectionId;
            _clusterId = clusterId;
            _numberOfindividuals = numberOfindividual;
            _numberOfClusters = numberOfClusters;
            _numberOfComponents = numberOfComponents;
            Height = 400;
            Width = 400;
        }

        public override string ChartId {
            get {
                var pictureId = "483fa8e9-2ff9-42ba-935a-3374aa6ecdfd";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _clusterId);
            }
        }
        public override string Title => $"Co-exposure of individuals: cluster {_clusterId} (n = {_numberOfindividuals}).";

        public override PlotModel Create() {
            var numberOfIndividuals = _components.Count/_numberOfComponents;

            var data = new double[_numberOfComponents, numberOfIndividuals];
            for (int i = 0; i < _numberOfComponents; i++) {
                var result = _components.Where(c => c.IdComponent == i).Select(c => c.NmfValue).ToList();
                for (int ix = 0; ix < result.Count; ix++) {
                    data[i, ix] = result.ElementAt(ix);
                }
            }
            return create(data, _numberOfClusters);
        }

        private static PlotModel create(double[,] matrix, int numberOfClusters) {
            var plotModel = new PlotModel() {
                IsLegendVisible = false,
            };

            var heatMapSeries = new HeatMapSeries() {
                Interpolate = false,
                X0 = 0,
                X1 = matrix.GetLength(0),
                Y0 = 0,
                Y1 = matrix.GetLength(1),
                LabelFontSize = 0,
                Data = matrix,
                RenderMethod = HeatMapRenderMethod.Rectangles,
            };
            plotModel.PlotAreaBorderThickness = new OxyThickness(1, 1, 1, 1);
            plotModel.PlotMargins = new OxyThickness(double.NaN, double.NaN, 75, double.NaN);

            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.HighColor = OxyColors.Red;
            linearColorAxis1.LowColor = OxyColors.White;
            linearColorAxis1.Position = AxisPosition.Right;
            linearColorAxis1.Palette = OxyPalette.Interpolate(100, OxyColors.White, OxyColors.Yellow, OxyColors.Red);
            linearColorAxis1.Palette.Colors[0] = OxyColor.FromAColor(100, OxyColor.FromRgb(204, 229, 255));
            plotModel.Axes.Add(linearColorAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.Title = "K";
            linearAxis2.IsAxisVisible = false;
            linearAxis2.TickStyle = TickStyle.None;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.TitlePosition = 1;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.MinimumPadding = 0;
            plotModel.Axes.Add(linearAxis2);


            var linearAxis3 = new LinearAxis();
            linearAxis3.TickStyle = TickStyle.None;
            linearAxis3.Position = AxisPosition.Left;
            linearAxis3.IsAxisVisible = false;
            linearAxis3.TextColor = OxyColors.Transparent;
            plotModel.Axes.Add(linearAxis3);


            var categoryAxis2 = new CategoryAxis();
            categoryAxis2.MinorStep = 1;
            categoryAxis2.Position = AxisPosition.Bottom;
            for (int i = 0; i < matrix.GetLength(0); i++) {
                var labelK = $"{i + 1}";
                categoryAxis2.Labels.Add(labelK);
            }
            plotModel.Axes.Add(categoryAxis2);

            if (numberOfClusters > 1) {
                var categoryAxis1 = new CategoryAxis();
                categoryAxis1.MinorStep = 1;
                categoryAxis1.Position = AxisPosition.Left;
                for (int i = numberOfClusters; i > 0; i--) {
                    var cluster = $"cluster {i}";
                    categoryAxis1.Labels.Add(cluster);
                }
                plotModel.Axes.Add(categoryAxis1);
            }

            plotModel.Series.Add(heatMapSeries);
            return plotModel;
        }
    }
}

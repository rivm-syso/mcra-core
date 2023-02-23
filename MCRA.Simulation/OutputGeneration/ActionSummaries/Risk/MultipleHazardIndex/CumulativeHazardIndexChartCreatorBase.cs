using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public class CumulativeHazardIndexChartCreatorBase : OxyPlotChartCreator {

        public override string ChartId => throw new System.NotImplementedException();
        public override PlotModel Create() {
            throw new System.NotImplementedException();
        }

        protected static PlotModel create(
            List<double> hiStatisticsPositives,
            List<string> substances,
            double rpfWeightedHI,
            double percentage,
            bool isUncertainty
        ) {
            var plotModel = new PlotModel();
            var numberOfRecords = hiStatisticsPositives.Count;
            var areaSeries1 = new AreaSeries() {
                Color = OxyColors.Red,
                Color2 = OxyColors.Red,
                Fill = OxyColor.FromAColor(135, OxyColors.Red),
                StrokeThickness = 1,
            };
            var categoryAxis1 = new CategoryAxis() {
                Position = AxisPosition.Bottom,
                Angle = 45,
                TickStyle = TickStyle.None,
                Minimum = -0.35,
            };
            var hazardIndices = new List<double> { 0 };

            hazardIndices.AddRange(hiStatisticsPositives);

            for (int i = 0; i < numberOfRecords; i++) {
                categoryAxis1.Labels.Add(substances[i]);
                areaSeries1.Points.Add(new DataPoint(i - 0.5, hazardIndices[i]));
                areaSeries1.Points.Add(new DataPoint(i + 0.5, hazardIndices[i]));
                areaSeries1.Points2.Add(new DataPoint(i - 0.35, hazardIndices[i]));
                areaSeries1.Points2.Add(new DataPoint(i - 0.35, hazardIndices[i + 1]));
            }
            areaSeries1.Points.Add(new DataPoint(numberOfRecords - 0.5, hazardIndices[numberOfRecords]));
            areaSeries1.Points2.Add(new DataPoint(numberOfRecords - 0.5, hazardIndices[numberOfRecords]));
            plotModel.Axes.Add(categoryAxis1);
            var yAxis = new LinearAxis() { Title = $"Cumulative HI {percentage:F0}" };
            plotModel.Axes.Add(yAxis);
            plotModel.Series.Add(areaSeries1);

            if (!double.IsNaN(rpfWeightedHI)) {
                //Add otherwise annotation is outside the plot
                var scatterSerie = new ScatterSeries() { MarkerType = MarkerType.None };
                scatterSerie.Points.Add(new ScatterPoint(0, rpfWeightedHI * 1.1));
                plotModel.Series.Add(scatterSerie);
                var annotation = new LineAnnotation() { Text = "Cumulative RPF weighted HI", Y = rpfWeightedHI, Type = LineAnnotationType.Horizontal, };
                plotModel.Annotations.Add(annotation);
            }
            return plotModel;
        }
    }
}

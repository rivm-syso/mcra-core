using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class CumulativeExposureThresholdRatioChartCreatorBase : OxyPlotChartCreator {

        protected static PlotModel create(
            List<double> riskStatisticsPositives,
            List<string> substances,
            double rpfWeightedRisk,
            double percentage,
            bool isUncertainty
        ) {
            var plotModel = new PlotModel();
            var numberOfRecords = riskStatisticsPositives.Count;
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
            var risks = new List<double> { 0 };

            risks.AddRange(riskStatisticsPositives);

            for (int i = 0; i < numberOfRecords; i++) {
                categoryAxis1.Labels.Add(substances[i]);
                areaSeries1.Points.Add(new DataPoint(i - 0.5, risks[i]));
                areaSeries1.Points.Add(new DataPoint(i + 0.5, risks[i]));
                areaSeries1.Points2.Add(new DataPoint(i - 0.35, risks[i]));
                areaSeries1.Points2.Add(new DataPoint(i - 0.35, risks[i + 1]));
            }
            areaSeries1.Points.Add(new DataPoint(numberOfRecords - 0.5, risks[numberOfRecords]));
            areaSeries1.Points2.Add(new DataPoint(numberOfRecords - 0.5, risks[numberOfRecords]));
            plotModel.Axes.Add(categoryAxis1);
            var yAxis = new LinearAxis() { Title = $"Cumulative risk {percentage:F0}" };
            plotModel.Axes.Add(yAxis);
            plotModel.Series.Add(areaSeries1);

            if (!double.IsNaN(rpfWeightedRisk)) {
                //Add otherwise annotation is outside the plot
                var scatterSerie = new ScatterSeries() { MarkerType = MarkerType.None };
                scatterSerie.Points.Add(new ScatterPoint(0, rpfWeightedRisk * 1.1));
                plotModel.Series.Add(scatterSerie);
                var annotation = new LineAnnotation() { Text = "Cumulative risk (RPF weighted)", Y = rpfWeightedRisk, Type = LineAnnotationType.Horizontal, };
                plotModel.Annotations.Add(annotation);
            }
            return plotModel;
        }
    }
}

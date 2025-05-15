using MCRA.General;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class TimeCourseChartCreatorBase : ReportLineChartCreatorBase {

        /// <summary>
        /// To get nice x-axis (multiples of 24 hours) the variable interval must be even.
        /// Steplength is related to resolution, so stepLength 2 means every 2 minutes if
        /// resolution is in hours stepLength 60 means every 60 minutes (1 hour) if resolution
        /// is in hours.
        /// </summary>
        protected PlotModel createPlotModel(
            List<double> xValues,
            List<double> yValues,
            TimeUnit timeScale,
            string xtitle,
            string ytitle,
            double globalMaximum,
            int numberOfDaysSkipped,
            bool useGlobalYMax
        ) {
            var plotModel = createDefaultPlotModel();

            double timeMultiplier;
            if (timeScale == TimeUnit.Days) {
                timeMultiplier = 1;
            } else if (timeScale == TimeUnit.Hours) {
                timeMultiplier = 24;
            } else if (timeScale == TimeUnit.Minutes) {
                timeMultiplier = 24 * 60;
            } else {
                throw new NotImplementedException();
            }

            var timeSpanAxis = new LinearAxis() {
                Position = AxisPosition.Bottom,
                Title = xtitle,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(timeSpanAxis);

            var verticalAxis = createLinearAxis(ytitle);
            verticalAxis.Maximum = useGlobalYMax
                ? globalMaximum
                : yValues.DefaultIfEmpty(1).Max() * 1.05;
            verticalAxis.Minimum = 0;
            plotModel.Axes.Add(verticalAxis);

            // Internal (target) exposure time series
            var series = new LineSeries() {
                Color = OxyColors.Red,
                MarkerType = MarkerType.None,
                StrokeThickness = 1.5,
            };
            for (var i = 0; i < xValues.Count; i++) {
                series.Points.Add(new DataPoint(xValues[i] / timeMultiplier, yValues[i]));
            }
            plotModel.Series.Add(series);

            // Vertical burn-in period reference line
            var burnInPeriodLineAnnotation = new LineAnnotation() {
                Type = LineAnnotationType.Vertical,
                X = numberOfDaysSkipped,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Black,
                StrokeThickness = 1
            };
            plotModel.Annotations.Add(burnInPeriodLineAnnotation);

            return plotModel;
        }
    }
}

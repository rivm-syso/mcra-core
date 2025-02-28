using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ConcentrationCorrelationsChartCreatorBase : ReportChartCreatorBase {

        public ConcentrationCorrelationsChartCreatorBase(
            int width,
            int height
        ) : base() {
            Width = width;
            Height = height;
        }

        protected ScatterSeries createScatterMask(OxyColor color) {
            return new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColor.FromArgb(125, color.R, color.G, color.B),
                MarkerSize = 3,
                MarkerStroke = color,
                MarkerStrokeThickness = 0.4,
            };
        }

        protected CustomScatterErrorSeries createCustomScatterErrorSeries() {
            return new CustomScatterErrorSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.White,
                MarkerSize = 3,
                MarkerStroke = OxyColors.White,
                MarkerStrokeThickness = 0.4,
                ErrorBarColor = OxyColor.FromArgb(125, 0, 0, 0),
                ErrorBarStrokeThickness = 1,
                ErrorBarStopWidth = 0,
            };
        }

        protected PlotModel CreateChart(
            string modelledExposureUnit,
            string monitoringConcentrationUnit,
            IEnumerable<(int NumRecords, bool BothPositive, bool BothZero, bool ZeroMonitoring, bool ZeroModelled,
                double Monitoring, double ModelledMedian, double[] ModelledPercentiles)> groupedExposures
        ) {
            var allZeroExposures = groupedExposures.Where(r => r.BothZero).ToList();
            var onlyPositiveModelled = groupedExposures.Where(r => r.ZeroMonitoring).ToList();
            var onlyPositiveMonitoring = groupedExposures.Where(r => r.ZeroModelled).ToList();
            var bothPositiveExposures = groupedExposures.Where(r => r.BothPositive).ToList();

            var positiveModelledValues = groupedExposures
                .SelectMany(r => r.ModelledPercentiles)
                .Where(r => r > 0)
                .ToList();
            var minModelledExposure = positiveModelledValues.Any()
                ? positiveModelledValues.Min() * (onlyPositiveMonitoring.Any() ? .1 : .8)
                : .0001;
            var maxModelledExposure = positiveModelledValues.Any()
                ? positiveModelledValues.Max() * 2
                : 10;

            var monitoringConcentrations = groupedExposures.Select(r => r.Monitoring).Where(r => r > 0).ToList();
            var minMonitoringConcentration = monitoringConcentrations.Any()
                ? monitoringConcentrations.Min() * (onlyPositiveModelled.Any() ? .1 : .8)
                : .0001;
            var maxMonitoringConcentration = monitoringConcentrations.Any()
                ? monitoringConcentrations.Max() * 2
                : 10;

            var plotModel = createDefaultPlotModel();

            var horizontalAxis = new LogarithmicAxis() {
                Title = $"Modelled ({modelledExposureUnit})",
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                Position = AxisPosition.Bottom,
                Minimum = minModelledExposure,
                Maximum = maxModelledExposure,
                Base = 10,
            };
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new LogarithmicAxis() {
                Title = $"Monitoring ({monitoringConcentrationUnit})",
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                Position = AxisPosition.Left,
                Minimum = minMonitoringConcentration,
                Maximum = maxMonitoringConcentration,
                Base = 10,
            };
            plotModel.Axes.Add(verticalAxis);

            var scatterSeries = createCustomScatterErrorSeries();
            var points = groupedExposures
                .Where(r => r.BothPositive)
                .Select(g => {
                    return new CustomScatterErrorPoint(
                        g.ModelledMedian, g.ModelledPercentiles[0], g.ModelledPercentiles[2],
                        g.Monitoring, double.NaN, double.NaN
                    );
                })
                .ToList();
            scatterSeries.Points.AddRange(points);
            plotModel.Series.Add(scatterSeries);

            var scatterSeriesmask = createScatterMask(OxyColors.Green);
            var maskPoints = groupedExposures
                .Where(r => r.BothPositive)
                .Select(g => {
                    return new ScatterPoint(
                        g.ModelledMedian, g.Monitoring
                    );
                })
                .ToList();
            scatterSeriesmask.Points.AddRange(maskPoints);
            plotModel.Series.Add(scatterSeriesmask);

            if (onlyPositiveModelled.Any()) {
                CustomScatterErrorSeries onlyPositiveModelledExposuresScatterSeries = createCustomScatterErrorSeries();
                var onlyPositiveModelledExposuresScatterPoints = onlyPositiveModelled
                    .Select(r => new CustomScatterErrorPoint(r.ModelledMedian, r.ModelledPercentiles[0],
                        r.ModelledPercentiles[2], minMonitoringConcentration * 1.1, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveModelledExposuresScatterSeries.Points.AddRange(onlyPositiveModelledExposuresScatterPoints);
                plotModel.Series.Add(onlyPositiveModelledExposuresScatterSeries);

                var onlyPositiveModelledExposuresScatterMaskSeries = createScatterMask(OxyColor.FromRgb(128, 128, 128));
                var onlyPositiveModelledExposuresScatterMaskPoints = onlyPositiveModelled
                    .Select(r => new CustomScatterErrorPoint(r.ModelledMedian, r.ModelledPercentiles[0],
                        r.ModelledPercentiles[2], minMonitoringConcentration * 1.1, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveModelledExposuresScatterMaskSeries.Points
                    .AddRange(onlyPositiveModelledExposuresScatterMaskPoints);
                plotModel.Series.Add(onlyPositiveModelledExposuresScatterMaskSeries);
            }

            if (onlyPositiveMonitoring.Any()) {
                var onlyPositiveMonitoringConcentrationsScatterSeries = createCustomScatterErrorSeries();
                var onlyPositiveMonitoringConcentrationsScatterPoints = onlyPositiveMonitoring
                    .Select(r => new CustomScatterErrorPoint(minModelledExposure * 1.1, double.NaN, double.NaN,
                        r.Monitoring, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveMonitoringConcentrationsScatterSeries.Points
                    .AddRange(onlyPositiveMonitoringConcentrationsScatterPoints);
                plotModel.Series.Add(onlyPositiveMonitoringConcentrationsScatterSeries);

                var onlyPositiveMonitoringConcentrationsScatterMaskSeries = createScatterMask(OxyColor.FromRgb(128, 128, 128));
                var onlyPositiveMonitoringConcentrationsScatterMaskPoints = onlyPositiveMonitoring
                    .Select(r => new CustomScatterErrorPoint(minModelledExposure * 1.1, double.NaN, double.NaN,
                    r.Monitoring, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveMonitoringConcentrationsScatterMaskSeries.Points
                    .AddRange(onlyPositiveMonitoringConcentrationsScatterMaskPoints);
                plotModel.Series.Add(onlyPositiveMonitoringConcentrationsScatterMaskSeries);
            }

            double[] factors = [.1, 1, 10];
            foreach (var factor in factors) {
                var lineSeriesAbsorption1 = new LineSeries() {
                    Color = factor == 1 ? OxyColors.Black : OxyColors.DarkGray,
                    LineStyle = LineStyle.Dash,
                    StrokeThickness = 1,
                };
                var minimum = .1 * Math.Min(minModelledExposure, minMonitoringConcentration);
                var maximum = 10 * Math.Min(maxModelledExposure, maxMonitoringConcentration);
                lineSeriesAbsorption1.Points.Add(new DataPoint(factor * minimum, minimum));
                lineSeriesAbsorption1.Points.Add(new DataPoint(factor * maximum, maximum));
                plotModel.Series.Add(lineSeriesAbsorption1);
            }

            return plotModel;
        }
    }
}

using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public class IndividualConcentrationCorrelationCumulativeChartCreator : ConcentrationCorrelationsChartCreatorBase {

        protected IndividualConcentrationCorrelationsCumulativeSection _section;
        protected string _codeSubstance;
        protected string _modelledExposureUnit;
        protected string _monitoringConcentrationUnit;
        protected double _lowerPercentage;
        protected double _upperPercentage;
        private string _nameSubstance;

        public IndividualConcentrationCorrelationCumulativeChartCreator(
            IndividualConcentrationCorrelationsCumulativeSection section,
            string codeSubstance,
            string modelledExposureUnit,
            string monitoringExposureUnit,
            double lowerPercentage,
            double upperPercentage,
            int width,
            int height
        ) : base(width, height) {
            _section = section;
            _codeSubstance = codeSubstance;
            _modelledExposureUnit = modelledExposureUnit;
            _monitoringConcentrationUnit = monitoringExposureUnit;
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _nameSubstance = section.Records.First(r => r.SubstanceCode == codeSubstance).SubstanceName;
        }
        public override string Title => $"Monitoring versus modelled (p{_lowerPercentage}, p{50}, p{_upperPercentage}) exposures {_nameSubstance}";

        public override string ChartId {
            get {
                var chartId = "805d8179-0e5c-4a51-bb98-8375362935a0";
                return StringExtensions.CreateFingerprint(_section.SectionId + chartId + _codeSubstance);
            }
        }

        public override PlotModel Create() {
            return createPlotModel(_section, _codeSubstance, _modelledExposureUnit, _monitoringConcentrationUnit);
        }

        protected virtual PlotModel createPlotModel(IndividualConcentrationCorrelationsCumulativeSection section, string codeSubstance, string modelledExposureUnit, string monitoringConcentrationUnit) {
            var record = section.Records.First(r => r.SubstanceCode == codeSubstance);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var groupedExposures = record.MonitoringVersusModelExposureRecords
                .GroupBy(r => r.Individual)
                .Select(g => {
                    var modelled = g.Select(r => r.ModelledExposure).ToList();
                    var monitoring = g.Select(r => r.MonitoringConcentration).Average();
                    var modelledPercentiles = modelled.Percentiles(percentages);
                    var modelledMedian = modelledPercentiles[1];
                    return (
                        NumRecords: g.Count(),
                        BothPositive: modelledMedian > 0 && monitoring > 0,
                        BothZero: modelledMedian <= 0 && monitoring <= 0,
                        ZeroMonitoring: modelledMedian > 0 && monitoring <= 0,
                        ZeroModelled: modelledMedian <= 0 && monitoring > 0,
                        Monitoring: monitoring,
                        ModelledMedian: modelledMedian,
                        ModelledPercentiles: g.Count() > 1
                            ? modelledPercentiles
                            : new double[] { double.NaN, modelled.First(), double.NaN }
                    );
                });

            var allZeroExposures = groupedExposures.Where(r => r.BothZero).ToList();
            var onlyPositiveModelled = groupedExposures.Where(r => r.ZeroMonitoring).ToList();
            var onlyPositiveMonitoring = groupedExposures.Where(r => r.ZeroModelled).ToList();
            var bothPositiveExposures = groupedExposures.Where(r => r.BothPositive).ToList();

            var positiveModelledValues = groupedExposures.SelectMany(r => r.ModelledPercentiles).Where(r => r > 0).ToList();
            var minModelledExposure = positiveModelledValues.Any() ? positiveModelledValues.Min() * (onlyPositiveMonitoring.Any() ? .1 : .8) : .0001;
            var maxModelledExposure = positiveModelledValues.Any() ? positiveModelledValues.Max() * 2 : 10;

            var monitoringConcentrations = groupedExposures.Select(r => r.Monitoring).Where(r => r > 0).ToList();
            var minMonitoringConcentration = monitoringConcentrations.Any() ? monitoringConcentrations.Min() * (onlyPositiveModelled.Any() ? .1 : .8) : .0001;
            var maxMonitoringConcentration = monitoringConcentrations.Any() ? monitoringConcentrations.Max() * 2 : 10;

            var plotModel = createDefaultPlotModel();

            var horizontalAxis = new LogarithmicAxis() {
                Title = $"Model ({modelledExposureUnit})",
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
                        g.ModelledMedian, g.ModelledPercentiles[0], g.ModelledPercentiles[2], g.Monitoring, double.NaN, double.NaN
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
                    .Select(r => new CustomScatterErrorPoint(r.ModelledMedian, r.ModelledPercentiles[0], r.ModelledPercentiles[2], minMonitoringConcentration * 1.1, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveModelledExposuresScatterSeries.Points.AddRange(onlyPositiveModelledExposuresScatterPoints);
                plotModel.Series.Add(onlyPositiveModelledExposuresScatterSeries);

                var onlyPositiveModelledExposuresScatterMaskSeries = createScatterMask(OxyColor.FromRgb(128, 128, 128));
                var onlyPositiveModelledExposuresScatterMaskPoints = onlyPositiveModelled
                    .Select(r => new CustomScatterErrorPoint(r.ModelledMedian, r.ModelledPercentiles[0], r.ModelledPercentiles[2], minMonitoringConcentration * 1.1, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveModelledExposuresScatterMaskSeries.Points.AddRange(onlyPositiveModelledExposuresScatterMaskPoints);
                plotModel.Series.Add(onlyPositiveModelledExposuresScatterMaskSeries);
            }

            if (onlyPositiveMonitoring.Any()) {
                var onlyPositiveMonitoringConcentrationsScatterSeries = createCustomScatterErrorSeries();
                var onlyPositiveMonitoringConcentrationsScatterPoints = onlyPositiveMonitoring
                    .Select(r => new CustomScatterErrorPoint(minModelledExposure * 1.1, double.NaN, double.NaN, r.Monitoring, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveMonitoringConcentrationsScatterSeries.Points.AddRange(onlyPositiveMonitoringConcentrationsScatterPoints);
                plotModel.Series.Add(onlyPositiveMonitoringConcentrationsScatterSeries);

                var onlyPositiveMonitoringConcentrationsScatterMaskSeries = createScatterMask(OxyColor.FromRgb(128, 128, 128));
                var onlyPositiveMonitoringConcentrationsScatterMaskPoints = onlyPositiveMonitoring
                    .Select(r => new CustomScatterErrorPoint(minModelledExposure * 1.1, double.NaN, double.NaN, r.Monitoring, double.NaN, double.NaN))
                    .ToList();
                onlyPositiveMonitoringConcentrationsScatterMaskSeries.Points.AddRange(onlyPositiveMonitoringConcentrationsScatterMaskPoints);
                plotModel.Series.Add(onlyPositiveMonitoringConcentrationsScatterMaskSeries);
            }

            var factors = new double[] { .1, 1, 10 };
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

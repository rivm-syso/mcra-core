using MCRA.General;
using MCRA.Simulation.Constants;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposure_EllipsChartCreator : HazardExposureHeatMapCreatorBase {

        private readonly bool _plotLines;

        public HazardExposure_EllipsChartCreator(
            HazardExposureSection section,
            TargetUnit targetUnit,
            bool plotLines = false
        ) : base(section, targetUnit) {
            _plotLines = plotLines;
        }

        public override string ChartId {
            get {
                var pictureId = "3aee1546-f069-435d-922d-6cf3f90da0e7";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + (_plotLines ? "Y" : "N"));
            }
        }

        public override PlotModel Create() {
            return create(_section);
        }

        private PlotModel create(HazardExposureSection section) {
            var records = getHazardExposureRecords(section, _targetUnit.Target);
            var plotModel = createPlotModel(section, records, _targetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendExpressionType));
            records = records.Take(section.NumberOfLabels).ToList();

            var decades = Math.Ceiling(Math.Log10(_yHigh)) - Math.Floor(Math.Log10(_yLow));
            var positionBottomLabel = Math.Pow(10, .2 * decades) * _yLow;
            var ticks = GetTicks(_xLow, _xHigh, records.Take(section.NumberOfLabels).ToList());
            var basePalette = OxyPalettes.Rainbow(records.Count == 1 ? 2 : records.Count);
            var palette = basePalette.Colors.Select(c => OxyColor.FromAColor(75, c));

            var edChiSq = Math.Sqrt(ChiSquaredDistribution.InvCDF(2, section.ConfidenceInterval / 100));

            var maxEllips = new List<double>();
            var xNominal = new List<List<double>>();
            var yNominal = new List<List<double>>();
            var xUncertainty = new List<List<double>>();
            var yUncertainty = new List<List<double>>();

            records = records.OrderByDescending(c => c.UpperExposure).ToList();
            var counter = 0;
            foreach (var record in records) {
                var sdCED2 = Math.Log(Math.Pow(record.StDevHc, 2) + 1);
                var sdExp2 = Math.Log(Math.Pow(record.StDevExposure, 2) + 1);
                var a = edChiSq * Math.Sqrt(sdExp2);
                var b = edChiSq * Math.Sqrt(sdCED2);
                var x = new List<double>();
                var y = new List<double>();
                var lineSeriesVariability = new AreaSeries() {
                    Fill = OxyColors.White,
                };
                for (int i = 0; i < 360; i++) {
                    var grid = i * (2 * Math.PI) / 359;
                    x.Add(Math.Exp(a * Math.Cos(grid) + Math.Log(record.MedianExposure)));
                    y.Add(Math.Exp(b * Math.Sin(grid) + Math.Log(record.MedianHc)));
                    lineSeriesVariability.Points.Add(new DataPoint(x[i], y[i]));
                }
                maxEllips.Add(x.Max());
                xNominal.Add(x);
                yNominal.Add(y);
                plotModel.Series.Add(lineSeriesVariability);

                if (_isUncertainty) {
                    var edSqrtChiSqUnc = Math.Sqrt(ChiSquaredDistribution.InvCDF(2, (1 - (1 - section.UncertaintyUpperLimit / 100) * 2)));
                    var edNormal = NormalDistribution.InvCDF(0, 1, section.UncertaintyUpperLimit / 100);
                    var sigmaUExp = Math.Log(record.UpperExposure_UncUpper / record.UpperExposure) / edNormal;
                    var sigmaUCED = -Math.Log(record.LowerHc_UncLower / record.LowerHc) / edNormal;
                    a = edChiSq * Math.Sqrt(sdExp2) + edSqrtChiSqUnc * sigmaUExp;
                    b = edChiSq * Math.Sqrt(sdCED2) + edSqrtChiSqUnc * sigmaUCED;
                    var xu = new List<double>();
                    var yu = new List<double>();
                    var areaSeriesUncertainty = new AreaSeries() {
                        Fill = OxyColors.White,
                    };
                    for (int i = 0; i < 360; i++) {
                        var grid = i * (2 * Math.PI) / 359;
                        xu.Add(Math.Exp(a * Math.Cos(grid) + Math.Log(record.MedianExposure)));
                        yu.Add(Math.Exp(b * Math.Sin(grid) + Math.Log(record.MedianHc)));
                        areaSeriesUncertainty.Points.Add(new DataPoint(xu[i], yu[i]));
                    }
                    xUncertainty.Add(xu);
                    yUncertainty.Add(yu);
                    //only cumulative equivalent
                    if (record.IsCumulativeRecord) {
                        plotModel.Series.Add(areaSeriesUncertainty);
                    }
                }
                counter++;
            }
            counter = 0;

            //Fill plot area whit colors, do the same for the uncertainty of the cumulative equivalent, but not the other substances
            foreach (var record in records) {
                var lineSeriesVariability = new AreaSeries() {
                    Title = record.SubstanceName,
                    Color = basePalette.Colors.ElementAt(counter),
                    Fill = palette.ElementAt(counter),
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid,
                };
                if (record.IsCumulativeRecord) {
                    lineSeriesVariability.Color = OxyColors.Red;
                    lineSeriesVariability.Fill = OxyColor.FromAColor(75, OxyColors.Red);
                }
                for (int i = 0; i < 360; i++) {
                    lineSeriesVariability.Points.Add(new DataPoint(xNominal[counter][i], yNominal[counter][i]));
                }
                plotModel.Series.Add(lineSeriesVariability);

                if (_isUncertainty) {
                    if (!record.IsCumulativeRecord) {
                        var lineSeriesUncertainty = new LineSeries() {
                            Title = record.SubstanceName,
                            Color = basePalette.Colors.ElementAt(counter),
                            StrokeThickness = 2,
                            LineStyle = LineStyle.Solid,
                        };
                        for (int i = 0; i < 360; i++) {
                            lineSeriesUncertainty.Points.Add(new DataPoint(xUncertainty[counter][i], yUncertainty[counter][i]));
                        }
                        plotModel.Series.Add(lineSeriesUncertainty);
                    } else {
                        var areaSeriesUncertainty = new AreaSeries() {
                            Title = record.SubstanceName,
                            Color = OxyColors.Red,
                            Fill = OxyColor.FromAColor(75, OxyColors.Red),
                            StrokeThickness = 2,
                            LineStyle = LineStyle.Solid,
                        };
                        for (int i = 0; i < 360; i++) {
                            areaSeriesUncertainty.Points.Add(new DataPoint(xUncertainty[counter][i], yUncertainty[counter][i]));
                        }
                        plotModel.Series.Add(areaSeriesUncertainty);
                    }
                }
                counter++;
            }

            counter = 0;
            foreach (var item in records) {
                var color = OxyColors.Black;
                var strokeThickness = 1;
                var label = (records.Count > 1)
                    ? item.SubstanceName
                    : string.Empty;
                var fontSize = 10;

                if (item.IsCumulativeRecord) {
                    color = OxyColors.Red;
                    strokeThickness = 3;
                    label = "CUMULATIVE";
                }

                var xl = item.LowerExposure == 0 ? _xLow : item.LowerExposure;
                var coordLower0 = new List<double> { xl, item.MedianHc };
                var coordUpper0 = new List<double> { item.UpperExposure, item.MedianHc };
                var lineSeriesExposure = createLineSeries(color, strokeThickness, coordLower0, coordUpper0, LineStyle.Solid);
                if (_plotLines) {
                    plotModel.Series.Add(lineSeriesExposure);
                }

                if (_isUncertainty) {
                    var xlu0 = item.LowerExposure_UncLower == 0 ? _xLow : item.LowerExposure_UncLower;
                    var coordLowerU0 = new List<double> { xlu0, item.MedianHc };
                    var coordUpperU0 = new List<double> { item.LowerExposure, item.MedianHc };
                    var lineSeriesExposureU0 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU0, coordUpperU0, LineStyle.Solid);
                    if (_plotLines) {
                        plotModel.Series.Add(lineSeriesExposureU0);
                    }
                    var coordLowerU1 = new List<double> { item.UpperExposure, item.MedianHc };
                    var coordUpperU1 = new List<double> { item.UpperExposure_UncUpper, item.MedianHc };
                    var lineSeriesExposureU1 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU1, coordUpperU1, LineStyle.Solid);
                    if (_plotLines) {
                        plotModel.Series.Add(lineSeriesExposureU1);
                    }
                }
                coordLower0 = new List<double> { item.MedianExposure, item.LowerHc };
                coordUpper0 = new List<double> { item.MedianExposure, item.UpperHc };
                var lineSeriesCED = createLineSeries(color, strokeThickness, coordLower0, coordUpper0, LineStyle.Solid);
                if (_plotLines) {
                    plotModel.Series.Add(lineSeriesCED);
                }

                if (_isUncertainty) {
                    var coordLowerU0 = new List<double> { item.MedianExposure, item.LowerHc };
                    var coordUpperU0 = new List<double> { item.MedianExposure, item.LowerHc_UncLower };
                    var lineSeriesExposureU0 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU0, coordUpperU0, LineStyle.Solid);
                    if (_plotLines) {
                        plotModel.Series.Add(lineSeriesExposureU0);
                    }
                    var coordLowerU1 = new List<double> { item.MedianExposure, item.UpperHc };
                    var coordUpperU1 = new List<double> { item.MedianExposure, item.UpperHc_UncUpper };
                    var lineSeriesExposureU1 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU1, coordUpperU1, LineStyle.Solid);
                    if (_plotLines) {
                        plotModel.Series.Add(lineSeriesExposureU1);
                    }
                }

                var spikeTER = false;
                if (item.PercentagePositives <= (100 - _percentage)) {
                    spikeTER = false;
                }
                var lowerRisk = item.LowerRisk == 0 ? 1 / SimulationConstants.MOE_eps : item.LowerRisk;
                var lowerRiskUnc = item.LowerRisk_UncLower == 0 ? 1 / SimulationConstants.MOE_eps : item.LowerRisk_UncLower;
                var upperRisk = item.UpperRisk;
                var upperRiskUnc = item.UpperRisk_UncUpper;
                if (section.RiskMetricType != RiskMetricType.HazardExposureRatio) {
                    lowerRisk = 1 / item.UpperRisk;
                    lowerRiskUnc = 1 / item.UpperRisk_UncUpper;
                    upperRisk = 1 / (item.LowerRisk == 0 ? 1 / SimulationConstants.MOE_eps : item.LowerRisk);
                    upperRiskUnc = 1 / (item.LowerRisk_UncLower == 0 ? 1 / SimulationConstants.MOE_eps : item.LowerRisk_UncLower);
                }

                var coordLower = GetCoordinates(_xLow, item.MedianExposure, item.MedianHc, upperRisk, spikeTER);
                var coordUpper = GetCoordinates(_xLow, item.MedianExposure, item.MedianHc, lowerRisk, false);
                var lineSeriesDiagonal = createLineSeries(color, strokeThickness, coordLower, coordUpper, LineStyle.Solid);
                if (_plotLines) {
                    plotModel.Series.Add(lineSeriesDiagonal);
                }

                if (_isUncertainty) {
                    var coordLowerU0 = GetCoordinates(_xLow, item.MedianExposure, item.MedianHc, upperRiskUnc, spikeTER);
                    var coordUpperU0 = GetCoordinates(_xLow, item.MedianExposure, item.MedianHc, upperRisk, false);
                    var lineSeriesDiagonalU0 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU0, coordUpperU0, LineStyle.Solid);
                    if (_plotLines) {
                        plotModel.Series.Add(lineSeriesDiagonalU0);
                    }
                    var coordLowerU1 = GetCoordinates(_xLow, item.MedianExposure, item.MedianHc, lowerRiskUnc, false);
                    var coordUpperU1 = GetCoordinates(_xLow, item.MedianExposure, item.MedianHc, lowerRisk, false);
                    var lineSeriesDiagonalU1 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU1, coordUpperU1, LineStyle.Solid);
                    if (_plotLines) {
                        plotModel.Series.Add(lineSeriesDiagonalU1);
                    }
                }

                if (label != string.Empty && counter < section.NumberOfLabels) {
                    if (item.IsCumulativeRecord) {
                        color = OxyColors.Red;
                    } else {
                        color = basePalette.Colors.ElementAt(counter);
                    }
                    var textAnnotation = createAnnotation(positionBottomLabel, ticks, counter, label, fontSize);
                    plotModel.Annotations.Add(textAnnotation);
                    var labelConnection = createLabelConnection(positionBottomLabel, ticks[counter], new List<double>() { maxEllips[counter], item.MedianHc }, color);
                    plotModel.Series.Add(labelConnection);
                }
                counter++;
            }
            return plotModel;
        }
    }
}

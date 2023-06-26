using MCRA.Utils.Charting.OxyPlot;
using MCRA.General;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HazardExposureHeatMapCreatorBase : OxyPlotHeatMapCreator {

        protected OxyColor colorUncertainty = OxyColors.White;
        protected int strikeThicknessUnc = 2;
        protected double eps = 1e-8;
        protected HazardExposureSection _section;
        protected string _intakeUnit;

        protected List<HazardExposureRecord> _hazardExposureRecords { get; set; }
        protected bool _isUncertainty { get; set; }

        protected double _percentage { get; set; }
        protected double _xLow { get; set; }
        protected double _xHigh { get; set; }
        protected double _yLow { get; set; }
        protected double _yHigh { get; set; }

        public HazardExposureHeatMapCreatorBase(
            HazardExposureSection section,
            string intakeUnit
        ) {
            Width = 600;
            Height = 600;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        protected PlotModel createPlotModel(HazardExposureSection section, string intakeUnit) {
            _isUncertainty = section.HasUncertainty;
            _percentage = (100 - section.ConfidenceInterval) / 2;
            if (section.RiskMetricType == RiskMetricType.HazardIndex) {
                _hazardExposureRecords = section.HazardExposureRecords
                    .Where(c => c.PercentagePositives > (100 - section.ConfidenceInterval) / 2)
                    .OrderByDescending(r => r.IsCumulativeRecord)
                    .ThenByDescending(c => c.UpperRisk)
                    .ToList();
            } else {
                _hazardExposureRecords = section.HazardExposureRecords
                    .Where(c => c.PercentagePositives > (100 - section.ConfidenceInterval) / 2)
                    .OrderByDescending(r => r.IsCumulativeRecord)
                    .ThenBy(c => c.LowerRisk)
                    .ToList();
            }
            var plotModel = createDefaultPlotModel();
            var sqrtChiSquare = Math.Sqrt(5.991);
            var bounds = getSmartBounds(_hazardExposureRecords, sqrtChiSquare);

            _xLow = bounds[0];
            _xHigh = bounds[1];
            _yLow = bounds[2];
            _yHigh = bounds[3];

            var horizontalAxis = createHorizontalLogarithmicAxis(_xLow, _xHigh);
            horizontalAxis.Title = $"Exposure ({intakeUnit})";
            horizontalAxis.FontWeight = 40;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createVerticalLogarithmicAxis(_yLow, _yHigh);
            verticalAxis.StartPosition = 1;
            verticalAxis.EndPosition = 0;
            verticalAxis.Title = $"Hazard characterisation ({intakeUnit})";
            verticalAxis.FontWeight = 40;


            plotModel.Axes.Add(verticalAxis);

            var slopeDirectionMultiplier = (section.HealthEffectType == HealthEffectType.Risk) ? 1D : -1D;
            var heatMapSeries = new HorizontalHeatMapSeries() {
                XLow = _xLow,
                XHigh = _xHigh,
                YLow = _yLow,
                YHigh = _yHigh,
                HeatMapMappingFunction = (x, y) => {
                    var z = y / (x * section.Threshold);
                    var desirabilityX = z / (1 + z);
                    var desirability = slopeDirectionMultiplier * (2 * desirabilityX - 1);
                    return desirability;
                },
            };
            plotModel.Series.Add(heatMapSeries);
            var lineSeries1 = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .6,
                LineStyle = LineStyle.Solid,
            };
            lineSeries1.Points.Add(new DataPoint(_xHigh, _xHigh));
            lineSeries1.Points.Add(new DataPoint(_yLow, _yLow));
            plotModel.Series.Add(lineSeries1);

            var lineSeriesWhite = new LineSeries() {
                Color = OxyColors.White,
                StrokeThickness = 4,
                LineStyle = LineStyle.Solid,
            };
            lineSeriesWhite.Points.Add(new DataPoint(_xHigh, _xHigh * section.Threshold));
            lineSeriesWhite.Points.Add(new DataPoint(_yLow / section.Threshold, _yLow));
            plotModel.Series.Add(lineSeriesWhite);

            var lineSeries2 = new LineSeries() {
                Color = OxyColors.Black,
                StrokeThickness = .8,
                LineStyle = LineStyle.Solid,
            };
            lineSeries2.Points.Add(new DataPoint(_xHigh, _xHigh * section.Threshold));
            lineSeries2.Points.Add(new DataPoint(_yLow / section.Threshold, _yLow));
            plotModel.Series.Add(lineSeries2);

            return plotModel;
        }

        /// <summary>
        /// Scatter: displays the median 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="color"></param>
        /// <param name="strokeThickness"></param>
        /// <returns></returns>
        protected ScatterSeries createScatterSeries(double x, double y, OxyColor color) {
            var scatterSeriesExposure = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = color,
                MarkerFill = color
            };
            scatterSeriesExposure.Points.Add(new ScatterPoint(x == 0 ? _xLow : x, y));
            return scatterSeriesExposure;
        }

        /// <summary>
        /// Diagonal line: IMOE
        /// </summary>
        /// <param name="color"></param>
        /// <param name="strokeThickness"></param>
        /// <param name="coordLower"></param>
        /// <param name="coordUpper"></param>
        /// <param name="lineStyle"></param>
        /// <returns></returns>
        protected LineSeries createLineSeries(
            OxyColor color,
            int strokeThickness,
            List<double> coordLower,
            List<double> coordUpper,
            LineStyle lineStyle
        ) {
            var lineSeries = new LineSeries() {
                Color = color,
                StrokeThickness = strokeThickness,
                LineStyle = lineStyle,
            };
            lineSeries.Points.Add(new DataPoint(coordLower[0], coordLower[1]));
            lineSeries.Points.Add(new DataPoint(coordUpper[0], coordUpper[1]));
            return lineSeries;
        }

        /// <summary>
        /// Label annotation
        /// </summary>
        /// <param name="positionBottomLabel"></param>
        /// <param name="ticks"></param>
        /// <param name="counter"></param>
        /// <param name="label"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        protected static TextAnnotation createAnnotation(
            double positionBottomLabel,
            List<double> ticks,
            int counter,
            string label,
            int fontSize
        ) {
            return new TextAnnotation() {
                Text = label,
                TextRotation = 270,
                TextPosition = new DataPoint(ticks[counter], positionBottomLabel),
                Stroke = OxyColors.Transparent,
                TextHorizontalAlignment = HorizontalAlignment.Left,
                TextColor = OxyColors.Black,
                TextVerticalAlignment = VerticalAlignment.Middle,
                Font = "Verdana",
                FontSize = fontSize,
                Background = OxyColors.Transparent,
            };
        }

        /// <summary>
        /// Segment to connect line with label.
        /// </summary>
        /// <param name="positionBottomLabel"></param>
        /// <param name="tick"></param>
        /// <param name="coordUpper"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        protected static LineSeries createLabelConnection(
            double positionBottomLabel,
            double tick,
            List<double> coordUpper,
            OxyColor color
        ) {
            var labelConnection = new LineSeries() {
                Color = color,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid
            };
            labelConnection.Points.Add(new DataPoint(coordUpper[0], coordUpper[1]));
            labelConnection.Points.Add(new DataPoint(tick, positionBottomLabel));
            return labelConnection;
        }

        /// <summary>
        /// Segment to connect line with label.
        /// Occasionally and I dont know why, the heatmap canvas is not plotted. 
        /// When the positionBottomLabel is increased by a factor 1.05 this helps.
        /// </summary>
        /// <param name="positionBottomLabel"></param>
        /// <param name="ticks"></param>
        /// <param name="counter"></param>
        /// <param name="coordUpper"></param>
        /// <returns></returns>
        protected static LineSeries createLabelConnection(
            double positionBottomLabel,
            List<double> ticks,
            int counter,
            List<double> coordUpper
        ) {
            var color = OxyColor.FromAColor(255, OxyColors.White);
            return createLabelConnection(positionBottomLabel, ticks[counter], coordUpper, color);
        }

        /// <summary>
        /// Equidistant ticks for labeling
        /// </summary>
        /// <param name="xLow"></param>
        /// <param name="xHigh"></param>
        /// <param name="cedExposureRecordsSet"></param>
        /// <returns></returns>
        protected static List<double> GetTicks(double xLow, double xHigh, List<HazardExposureRecord> cedExposureRecordsSet) {
            var tick = Math.Log10(xHigh / xLow) / (cedExposureRecordsSet.Count + .5);
            var ticks = new List<double>();
            var start = Math.Log10(xLow) + tick;
            for (int i = 0; i < cedExposureRecordsSet.Count; i++) {
                ticks.Add(Math.Pow(10, start + i * tick));
            }
            return ticks.OrderByDescending(c => c).ToList();
        }

        /// <summary>
        /// Calculates bounds such that both y-axis and x-axis have an equal
        /// number of decades and plotted lines are centered.
        /// </summary>
        /// <param name="hazardExposureRecords"></param>
        /// <param name="sqrtChiSquare"></param>
        /// <returns></returns>
        private List<double> getSmartBounds(
            List<HazardExposureRecord> hazardExposureRecords,
            double sqrtChiSquare
        ) {
            double xLow;
            double xHigh;
            double yLow;
            double yHigh;

            var bounds = calculateBivariateBounds(hazardExposureRecords, sqrtChiSquare);
            xLow = bounds[0] * 0.9;
            xHigh = bounds[1] * 1.1;
            yLow = bounds[2] * 0.9;
            yHigh = bounds[3] * 1.1;

            var logBounds = new List<double> {
                Math.Floor(Math.Log10(bounds[0])),
                Math.Ceiling(Math.Log10(bounds[1])),
                Math.Floor(Math.Log10(bounds[2])),
                Math.Ceiling(Math.Log10(bounds[3]))
            };

            var yAxisDecades = logBounds[3] - logBounds[2];
            var xAxisDecades = logBounds[1] - logBounds[0];

            if (yAxisDecades > xAxisDecades) {
                var shift = (yAxisDecades - xAxisDecades) / 2;
                xHigh = xLow * Math.Pow(10, yAxisDecades - shift);
                xLow = xLow / Math.Pow(10, shift);
            } else {
                var shift = (xAxisDecades - yAxisDecades) / 2;
                yLow = yHigh / Math.Pow(10, xAxisDecades - shift);
                yHigh = yHigh * Math.Pow(10, shift);
            }

            return new List<double> { xLow, xHigh, yLow, yHigh };
        }

        /// <summary>
        /// http://www.visiondummy.com/2014/04/draw-error-ellipse-representing-covariance-matrix/#Axis-aligned_confidence_ellipses
        /// </summary>
        /// <param name="hazardExposureRecords"></param>
        /// <param name="sqrtChiSquare2"></param>
        /// <returns></returns>
        private List<double> calculateBivariateBounds(
            List<HazardExposureRecord> hazardExposureRecords,
            double sqrtChiSquare2
        ) {
            var bounds = new List<double>();
            var minimumX = double.PositiveInfinity;
            var minimumY = double.PositiveInfinity;
            var maximumX = double.NegativeInfinity;
            var maximumY = double.NegativeInfinity;
            foreach (var item in hazardExposureRecords) {
                //var sdCED2 = Math.Log(Math.Pow(item.StDevCED, 2) + 1);
                //var sdExp2 = Math.Log(Math.Pow(item.StDevExposure, 2) + 1);
                //var sigmaUExp2 = item.PUpperExposure_uncertainty == 0 ? 0 : Math.Pow(Math.Log(item.PUpperExposure_uncertainty / item.MedianExposure) / 2, 2);
                //var sigmaUCED2 = item.PUpperCED_uncertainty == 0 ? 0 : Math.Pow(Math.Log(item.PUpperCED_uncertainty / item.MedianCED) / 2, 2);
                //var stDevExp = Math.Sqrt(sdExp2 + sigmaUExp2);
                //var stDevCED = Math.Sqrt(sdCED2 + sigmaUCED2);
                //var z = ((1 - Percentage / 100) - (100 - item.PercentagePositives) / 100) / (item.PercentagePositives / 100);
                //z = z < 0 ? 0 : z;
                //var minExposure = Math.Exp(Math.Log(item.MedianExposure) - z * stDevExp * sqrtChiSquare2);
                //var maxExposure = Math.Exp(Math.Log(item.MedianExposure) + z * stDevExp * sqrtChiSquare2);
                //var minCed = Math.Exp(Math.Log(item.MedianCED) - z * stDevCED * sqrtChiSquare2);
                //var maxCed = Math.Exp(Math.Log(item.MedianCED) + z * stDevCED * sqrtChiSquare2);

                var minExposure = Math.Min(item.LowerExposure, double.IsNaN(item.LowerExposure_UncLower) ? item.LowerExposure : item.LowerExposure_UncLower);
                if (minimumX > minExposure) {
                    minimumX = minExposure;
                }
                var maxExposure = Math.Max(item.UpperExposure, double.IsNaN(item.UpperExposure_UncUpper) ? item.UpperExposure : item.UpperExposure_UncUpper);
                if (maximumX < maxExposure) {
                    maximumX = maxExposure;
                }
                var minCed = Math.Min(item.LowerHc, double.IsNaN(item.LowerHc_UncLower) ? item.LowerHc : item.LowerHc_UncLower);
                if (minimumY > minCed) {
                    minimumY = minCed;
                }
                var maxCed = Math.Max(item.UpperHc, double.IsNaN(item.UpperHc_UncUpper) ? item.UpperHc : item.UpperHc_UncUpper);
                if (maximumY < maxCed) {
                    maximumY = maxCed;
                }
                minimumX = minimumX == 0 ? eps : minimumX;
            }
            bounds.Add(minimumX);
            bounds.Add(maximumX);
            bounds.Add(minimumY);
            bounds.Add(maximumY);
            return bounds;
        }

        /// <summary>
        /// Calculated coordinates for diagonal.
        /// </summary>
        /// <param name="xLow"></param>
        /// <param name="pUpperExposure"></param>
        /// <param name="y0Ref"></param>
        /// <param name="point"></param>
        /// <param name="spikeTER"></param>
        /// <returns></returns>
        protected static List<double> GetCoordinates(
            double xLow,
            double pUpperExposure,
            double y0Ref,
            double point,
            bool spikeTER
        ) {
            var x0 = Math.Log10(pUpperExposure);
            var y0 = Math.Log10(y0Ref);

            var cx0 = (y0 + x0 - Math.Log10(point)) / 2;
            if (spikeTER) {
                cx0 = Math.Log10(xLow);
                point = Math.Pow(10, -2 * cx0 + y0 + x0);
            }
            var cy0 = cx0 + Math.Log10(point);
            cx0 = Math.Pow(10, cx0);
            cy0 = Math.Pow(10, cy0);
            return new List<double> { cx0, cy0 };
        }
    }
}
﻿using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposureTERExpCedvsUpperExpLowerCedChartCreator : HazardExposureHeatMapCreatorBase {

        public HazardExposureTERExpCedvsUpperExpLowerCedChartCreator(
            HazardExposureSection section,
            TargetUnit targetUnit
        ) : base(section, targetUnit) {
        }

        public override string ChartId {
            get {
                var pictureId = "38f0e43b-25f2-48fe-b253-2c154f36c99d";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
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

            var counter = 0;
            foreach (var item in records) {
                var color = OxyColors.Black;
                var strokeThickness = 1;
                var label = item.SubstanceName;
                var fontSize = 10;

                if (item.IsCumulativeRecord) {
                    color = OxyColors.Red;
                    strokeThickness = 3;
                    label = "CUMULATIVE";
                    fontSize = 13;
                }

                var xl = item.LowerExposure == 0 ? _xLow : item.LowerExposure;
                var coordLower0 = new List<double> { xl, item.LowerHc };
                var coordUpper0 = new List<double> { item.UpperExposure, item.LowerHc };
                var lineSeriesExposure = createLineSeries(color, strokeThickness, coordLower0, coordUpper0, LineStyle.Solid);
                plotModel.Series.Add(lineSeriesExposure);

                if (_isUncertainty) {
                    var xlu0 = item.LowerExposure_UncLower == 0 ? _xLow : item.LowerExposure_UncLower;
                    var coordLowerU0 = new List<double> { xlu0, item.LowerHc };
                    var coordUpperU0 = new List<double> { item.LowerExposure, item.LowerHc };
                    var lineSeriesExposureU0 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU0, coordUpperU0, LineStyle.Solid);
                    plotModel.Series.Add(lineSeriesExposureU0);
                    var coordLowerU1 = new List<double> { item.UpperExposure, item.LowerHc };
                    var coordUpperU1 = new List<double> { item.UpperExposure_UncUpper, item.LowerHc };
                    var lineSeriesExposureU1 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU1, coordUpperU1, LineStyle.Solid);
                    plotModel.Series.Add(lineSeriesExposureU1);
                }

                var lineSeriesCED = createLineSeries(color, strokeThickness, coordUpper0, [item.UpperExposure, item.UpperHc], LineStyle.Solid);
                plotModel.Series.Add(lineSeriesCED);

                if (_isUncertainty) {
                    var coordLowerU0 = new List<double> { item.UpperExposure, item.LowerHc };
                    var coordUpperU0 = new List<double> { item.UpperExposure, item.LowerHc_UncLower };
                    var lineSeriesExposureU0 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU0, coordUpperU0, LineStyle.Solid);
                    plotModel.Series.Add(lineSeriesExposureU0);
                    var coordLowerU1 = new List<double> { item.UpperExposure, item.UpperHc };
                    var coordUpperU1 = new List<double> { item.UpperExposure, item.UpperHc_UncUpper };
                    var lineSeriesExposureU1 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU1, coordUpperU1, LineStyle.Solid);
                    plotModel.Series.Add(lineSeriesExposureU1);
                }

                var spikeRisk = false;
                if (item.PercentagePositives <= (100 - _percentage)) {
                    spikeRisk = true;
                }
                var lowerRisk = item.LowerRisk;
                var lowerRiskUnc = item.LowerRisk_UncLower;
                var upperRisk = item.UpperRisk;
                var upperRiskUnc = item.UpperRisk_UncUpper;
                if (section.RiskMetricType != RiskMetricType.HazardExposureRatio) {
                    lowerRisk = 1 / item.UpperRisk;
                    lowerRiskUnc = 1 / item.UpperRisk_UncUpper;
                    upperRisk = 1 / item.LowerRisk;
                    upperRiskUnc = 1 / item.LowerRisk_UncLower;
                }
                var coordLower = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, upperRisk, spikeRisk);
                var coordUpper = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, lowerRisk, false);
                var lineSeriesDiagonal = createLineSeries(color, strokeThickness, coordLower, coordUpper, LineStyle.Solid);
                plotModel.Series.Add(lineSeriesDiagonal);

                if (_isUncertainty) {
                    var coordLowerU0 = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, upperRiskUnc, spikeRisk);
                    var coordUpperU0 = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, upperRisk, false);
                    var lineSeriesDiagonalU0 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU0, coordUpperU0, LineStyle.Solid);
                    plotModel.Series.Add(lineSeriesDiagonalU0);
                    var coordLowerU1 = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, lowerRiskUnc, false);
                    var coordUpperU1 = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, lowerRisk, false);
                    var lineSeriesDiagonalU1 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU1, coordUpperU1, LineStyle.Solid);
                    plotModel.Series.Add(lineSeriesDiagonalU1);
                }
                if (label != string.Empty && counter < section.NumberOfLabels) {
                    var textAnnotation = createAnnotation(positionBottomLabel, ticks, counter, label, fontSize);
                    plotModel.Annotations.Add(textAnnotation);
                    var labelConnection = createLabelConnection(positionBottomLabel, ticks, counter, coordUpper);
                    plotModel.Series.Add(labelConnection);
                }
                counter++;
            }

            return plotModel;
        }
    }
}

﻿using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposure_ExpvsLowerCedChartCreator : HazardExposureHeatMapCreatorBase {

        public HazardExposure_ExpvsLowerCedChartCreator(
            HazardExposureSection section,
            TargetUnit targetUnit
        ) : base(section, targetUnit) {
        }

        public override string ChartId {
            get {
                var pictureId = "b08c515a-201e-4b21-a829-9c532322ba95";
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

            records = records.OrderByDescending(c => c.UpperExposure).ToList();
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
                var coordLower = new List<double> { xl, item.LowerHc };
                var coordUpper = new List<double> { item.UpperExposure, item.LowerHc };
                var lineSeriesExposure = createLineSeries(color, strokeThickness, coordLower, coordUpper, LineStyle.Solid);
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

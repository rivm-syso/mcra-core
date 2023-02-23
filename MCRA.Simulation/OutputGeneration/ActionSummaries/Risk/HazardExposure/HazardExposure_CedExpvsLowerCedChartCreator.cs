using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposure_CedExpvsLowerCedChartCreator : HazardExposureHeatMapCreatorBase {

        public HazardExposure_CedExpvsLowerCedChartCreator(
            HazardExposureSection section,
            string intakeUnit
        ) : base(section, intakeUnit) {
        }

        public override string ChartId {
            get {
                var pictureId = "95110693-bfc8-469c-9aa6-f80969347d00";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section);
        }

        private PlotModel create(HazardExposureSection section) {
            var plotModel = base.createPlotModel(section, _intakeUnit);
            var decades = Math.Ceiling(Math.Log10(_yHigh)) - Math.Floor(Math.Log10(_yLow));
            var positionBottomLabel = Math.Pow(10, .2 * decades) * _yLow;
            var ticks = GetTicks(_xLow, _xHigh, section.HazardExposureRecords.Take(section.NumberOfLabels).ToList());
            var numberOfSubstances = section.NumberOfSubstances;
            if (section.NumberOfSubstances <= section.NumberOfLabels) {
                numberOfSubstances = section.NumberOfLabels;
            }
            _hazardExposureRecords = _hazardExposureRecords.Take(numberOfSubstances).ToList();
            var counter = 0;
            foreach (var item in _hazardExposureRecords) {
                var color = OxyColors.Black;
                var strokeThickness = 1;
                var label = string.Empty;
                if (section.HazardExposureRecords.Count > 1) {
                    label = item.SubstanceName;
                }
                var fontSize = 10;

                if (item.IsCumulativeRecord) {
                    color = OxyColors.Red;
                    strokeThickness = 3;
                    label = "CUMULATIVE";
                    fontSize = 13;
                }

                var xl = item.LowerExposure == 0 ? _xLow : item.LowerExposure;
                var coordLower0 = new List<double> { xl, item.LowerHc };
                var coordUpper = new List<double> { item.UpperExposure, item.LowerHc };
                var lineSeriesExposure = createLineSeries(color, strokeThickness, coordLower0, coordUpper, LineStyle.Solid);
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

                var lineSeriesCED = createLineSeries(color, strokeThickness, coordUpper, new List<double> { item.UpperExposure, item.UpperHc }, LineStyle.Solid);
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

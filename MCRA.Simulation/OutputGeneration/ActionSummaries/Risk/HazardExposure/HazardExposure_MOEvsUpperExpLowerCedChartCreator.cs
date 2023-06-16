using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposure_MOEvsUpperExpLowerCedChartCreator : HazardExposureHeatMapCreatorBase {

        public HazardExposure_MOEvsUpperExpLowerCedChartCreator(
            HazardExposureSection section,
            string intakeUnit
        ) : base(section, intakeUnit) {
        }

        public override string ChartId {
            get {
                var pictureId = "165c073d-32b7-4d6c-b321-0a4268a36399";
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
            var ticks = GetTicks(_xLow, _xHigh, _hazardExposureRecords.Take(section.NumberOfLabels).ToList());
            var counter = 0;
            var numberOfSubstances = section.NumberOfSubstances;
            if (section.NumberOfSubstances <= section.NumberOfLabels) {
                numberOfSubstances = section.NumberOfLabels;
            }
            _hazardExposureRecords = _hazardExposureRecords.Take(numberOfSubstances).ToList();
            foreach (var item in _hazardExposureRecords) {
                var color = OxyColors.Black;
                var strokeThickness = 1;
                var label = string.Empty;
                if (_hazardExposureRecords.Count > 1) {
                    label = item.SubstanceName;
                }
                var fontSize = 10;

                if (item.IsCumulativeRecord) {
                    color = OxyColors.Red;
                    strokeThickness = 3;
                    label = "CUMULATIVE";
                    fontSize = 13;
                }

                var spikeTER = false;
                if (item.PercentagePositives <= (100 - _percentage)) {
                    spikeTER = true;
                }
                var lowerRisk = item.LowerRisk;
                var lowerRiskUnc = item.LowerRisk_UncLower;
                var upperRisk = item.UpperRisk;
                var upperRiskUnc = item.UpperRisk_UncUpper;
                if (section.RiskMetricType != RiskMetricType.MarginOfExposure) {
                    lowerRisk = 1 / item.UpperRisk;
                    lowerRiskUnc = 1 / item.UpperRisk_UncUpper;
                    upperRisk = 1 / item.LowerRisk;
                    upperRiskUnc = 1 / item.LowerRisk_UncLower;
                }

                var coordLower = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, upperRisk, spikeTER);
                var coordUpper = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, lowerRisk, false);
                var lineSeriesDiagonal = createLineSeries(color, strokeThickness, coordLower, coordUpper, LineStyle.Solid);

                plotModel.Series.Add(lineSeriesDiagonal);

                if (_isUncertainty) {
                    var coordLowerU0 = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, upperRiskUnc, spikeTER);
                    var coordUpperU0 = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, upperRisk, false);
                    var lineSeriesDiagonalU0 = createLineSeries(colorUncertainty, strikeThicknessUnc, coordLowerU0, coordUpperU0, LineStyle.Solid);
                    plotModel.Series.Add(lineSeriesDiagonalU0);
                    var coordLowerU1 = GetCoordinates(_xLow, item.UpperExposure, item.LowerHc, lowerRiskUnc , false);
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

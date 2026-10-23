using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {

    public class DerivedKineticConversionFactorModelScatterChartCreator : ReportLineChartCreatorBase {

        private readonly string _sectionId;
        private readonly DerivedKineticConversionFactorModelSummaryRecord _record;

        public DerivedKineticConversionFactorModelScatterChartCreator(
            DerivedKineticConversionFactorModelSummaryRecord record,
            string sectionId
        ) {
            Width = 350;
            Height = 250;
            _sectionId = sectionId;
            _record = record;
        }

        public override string Title {
            get {
                if (_record.IsExternalTargetFrom() && !_record.IsExternalTargetTo()) {
                    return $"External {_record.GetTargetNameTo()} exposures versus internal concentrations at {_record.GetTargetNameTo()}.";
                } else if (!_record.IsExternalTargetFrom() && !_record.IsExternalTargetTo()) {
                    return $"Internal concentrations at {_record.GetTargetNameTo()} versus internal concentrations at {_record.GetTargetNameTo()}.";
                } else if (!_record.IsExternalTargetFrom() && _record.IsExternalTargetTo()) {
                    return $"Internal concentrations at {_record.GetTargetNameTo()} versus external exposures at {_record.GetTargetNameTo()}.";
                } else {
                    return null;
                }
            }
        }

        public override string ChartId {
            get {
                var chartId = "6C3A6EAE-074E-4254-A421-ADB31371319A";
                return StringExtensions.CreateFingerprint(_sectionId + _record.GetKey() + chartId);
            }
        }

        public override PlotModel Create() {
            return create(_record);
        }

        protected PlotModel create(
            DerivedKineticConversionFactorModelSummaryRecord record
        ) {
            var plotModel = createDefaultPlotModel();

            var exposurePairs = record.ExposurePairs;
            var fromTargetExposures = exposurePairs.Select(r => r.ExposureFrom).ToList();
            var toTargetExposures = exposurePairs.Select(r => r.ExposureTo).ToList();
            var records = fromTargetExposures.Zip(toTargetExposures).ToList();

            var minExternalExposures = fromTargetExposures.Any(c => c > 0)
                ? fromTargetExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
            var maxExternalExposures = fromTargetExposures.Any(c => c > 0)
                ? fromTargetExposures.Where(c => c > 0).Max() * 2 : 2;
            var minInternalExposures = double.MinValue;
            var maxInternalExposures = double.MaxValue;
            minInternalExposures = toTargetExposures.Any(c => c > 0)
                ? toTargetExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
            maxInternalExposures = toTargetExposures.Any(c => c > 0)
                ? toTargetExposures.Where(c => c > 0).Max() * 1.1 : 2;

            var minimum = Math.Min(minInternalExposures, minExternalExposures);
            var maximum = Math.Max(maxInternalExposures, maxExternalExposures);

            var xLabel = $"{record.GetTargetNameFrom()} - {record.SubstanceNameFrom} ({record.UnitFrom})";
            var yLabel = $"{record.GetTargetNameTo()} - {record.SubstanceNameTo} ({record.UnitTo})";
            var horizontalAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = xLabel,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Minimum = minimum,
                Maximum = maximum,
            };
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new LogarithmicAxis() {
                Position = AxisPosition.Left,
                Title = yLabel,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Minimum = minimum,
                Maximum = maximum,
            };
            plotModel.Axes.Add(verticalAxis);

            var scatterSeries = new ScatterSeries() {
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerFill = OxyColor.FromArgb(125, 0, 128, 0),
                MarkerStroke = OxyColors.Green,
                MarkerStrokeThickness = 0.4,
            };
            foreach (var item in record.ExposurePairs) {
                scatterSeries.Points.Add(
                    new ScatterPoint(item.ExposureFrom, item.ExposureTo)
                );
            }
            plotModel.Series.Add(scatterSeries);

            // Neutral line series (x = y)
            var neutralFactorLineSeries = new LineSeries() {
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1,
            };
            neutralFactorLineSeries.Points.Add(new DataPoint(minimum, minimum));
            neutralFactorLineSeries.Points.Add(new DataPoint(maximum, maximum));
            plotModel.Series.Add(neutralFactorLineSeries);

            // Conversion factor
            if (records.Count > 1
                && fromTargetExposures.Distinct().Count() > 1
                && toTargetExposures.Distinct().Count() > 1
            ) {
                var conversionFactorLineSeries = new LineSeries() {
                    Color = OxyColors.Black,
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 1,
                };
                if (record.NominalFactor < 1) {
                    var xMin = minimum / record.NominalFactor;
                    var yMax = maximum * record.NominalFactor;
                    conversionFactorLineSeries.Points.Add(new DataPoint(xMin, minimum));
                    conversionFactorLineSeries.Points.Add(new DataPoint(maximum, yMax));
                } else {
                    var yMin = minimum * record.NominalFactor;
                    var xMax = maximum / record.NominalFactor;
                    conversionFactorLineSeries.Points.Add(new DataPoint(minimum, yMin));
                    conversionFactorLineSeries.Points.Add(new DataPoint(xMax, maximum));
                }
                plotModel.Series.Add(conversionFactorLineSeries);
            }
            return plotModel;
        }
    }
}

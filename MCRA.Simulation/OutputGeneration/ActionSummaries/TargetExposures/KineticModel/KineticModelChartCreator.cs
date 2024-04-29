using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;

namespace MCRA.Simulation.OutputGeneration {

    public class KineticModelChartCreator : ReportLineChartCreatorBase {

        private readonly KineticModelSection _section;
        private readonly string _compartment;
        private readonly string _externalExposureUnit;
        private readonly string _internalExposureUnit;


        public KineticModelChartCreator(
            KineticModelSection section,
            string compartment,
            string internalExposureUnit,
            string externalExposureUnit
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _externalExposureUnit = externalExposureUnit;
            _internalExposureUnit = internalExposureUnit;
            _compartment = compartment;
        }

        public override string Title => $"Internal ({_compartment}) versus external exposures of {_section.SubstanceName}.";

        public override string ChartId {
            get {
                var pictureId = "c651aee4-f5f5-49aa-8b14-cd6e33fa1d04";
                return StringExtensions.CreateFingerprint(_section.SectionId + _compartment + pictureId);
            }
        }

        public override PlotModel Create() {
            var xtitle = $"External exposure ({_internalExposureUnit})";
            var ytitle = $"Internal exposure ({_externalExposureUnit})";
            return createPlotModel(xtitle, ytitle);
        }

        protected PlotModel createPlotModel(string xtitle, string ytitle) {
            var plotModel = createDefaultPlotModel();

            var externalExposures = _section.ExternalExposures.Single(c => c.compartment == _compartment).Item2;
            var peakExposures = _section.PeakTargetExposures.Single(c => c.compartment == _compartment).Item2;
            var steadyStateExposures = _section.SteadyStateTargetExposures.Single(c => c.compartment == _compartment).Item2;


            var minExternalExposures = externalExposures.Any(c => c > 0) ? externalExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
            var maxExternalExposures = externalExposures.Any(c => c > 0) ? externalExposures.Where(c => c > 0).Max() * 2 : 2;
            var minInternalExposures = double.MinValue;
            var maxInternalExposures = double.MaxValue;
            if (_section.ExposureType == ExposureType.Acute) {
                minInternalExposures = peakExposures.Any(c => c > 0) ? peakExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
                maxInternalExposures = peakExposures.Any(c => c > 0) ? peakExposures.Where(c => c > 0).Max() * 1.1 : 2;
            } else {
                minInternalExposures = steadyStateExposures.Any(c => c > 0) ? steadyStateExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
                maxInternalExposures = steadyStateExposures.Any(c => c > 0) ? steadyStateExposures.Where(c => c > 0).Max() * 1.1 : 2;
            }
            var minimum = Math.Min(minInternalExposures, minExternalExposures);
            var maximum = Math.Max(maxInternalExposures, maxExternalExposures);

            var horizontalAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = xtitle,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Minimum = minimum,
                Maximum = maximum,
            };
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new LogarithmicAxis() {
                Position = AxisPosition.Left,
                Title = ytitle,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Minimum = minimum,
                Maximum = maximum,
            };
            plotModel.Axes.Add(verticalAxis);

            if (_section.ExposureType == ExposureType.Acute && peakExposures.Count == externalExposures.Count) {
                var data = new Collection<object>();
                for (int i = 0; i < externalExposures.Count; i++) {
                    data.Add(new {
                        PeakInternalValues = peakExposures[i],
                        ExternalValues = externalExposures[i]
                    });
                }
                var series2 = new ScatterSeries() {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 2,
                    MarkerFill = OxyColor.FromArgb(125, 0, 128, 0),
                    DataFieldY = "PeakInternalValues",
                    DataFieldX = "ExternalValues",
                    ItemsSource = data,
                    MarkerStroke = OxyColors.Green,
                    MarkerStrokeThickness = 0.4,
                };
                plotModel.Series.Add(series2);
            }

            if (_section.ExposureType == ExposureType.Chronic && steadyStateExposures.Count == externalExposures.Count) {
                var data = new Collection<object>();
                for (int i = 0; i < externalExposures.Count; i++) {
                    data.Add(new {
                        MeanInternalValues = steadyStateExposures[i],
                        ExternalValues = externalExposures[i]
                    });
                }
                var series1 = new ScatterSeries() {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 2,
                    MarkerFill = OxyColor.FromArgb(125, 65, 105, 225),
                    DataFieldY = "MeanInternalValues",
                    DataFieldX = "ExternalValues",
                    ItemsSource = data,
                    MarkerStroke = OxyColors.RoyalBlue,
                    MarkerStrokeThickness = 0.4,
                };
                plotModel.Series.Add(series1);
            }

            var lineSeriesAbsorption1 = new LineSeries() {
                Color = OxyColors.Black,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1,
            };

            lineSeriesAbsorption1.Points.Add(new DataPoint(minimum, minimum));
            lineSeriesAbsorption1.Points.Add(new DataPoint(maximum, maximum));
            plotModel.Series.Add(lineSeriesAbsorption1);
            return plotModel;
        }
    }
}

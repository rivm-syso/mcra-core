using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;

namespace MCRA.Simulation.OutputGeneration {

    public class KineticModelChartCreator : LineChartCreatorBase {

        private readonly KineticModelSection _section;
        private readonly string _externalExposureUnit;
        private readonly string _internalExposureUnit;

        public bool PlotLinearApproximationLine { get; set; } = false;

        public KineticModelChartCreator(
            KineticModelSection section,
            string internalExposureUnit,
            string externalExposureUnit
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _externalExposureUnit = externalExposureUnit;
            _internalExposureUnit = internalExposureUnit;
        }

        public override string Title => $"Internal versus external exposures of {_section.SubstanceName}.";

        public override string ChartId {
            get {
                var pictureId = "c651aee4-f5f5-49aa-8b14-cd6e33fa1d04";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section, _internalExposureUnit, _externalExposureUnit);
        }

        private PlotModel create(KineticModelSection section, string internalConcentrationUnit, string intakeUnit) {
            var xtitle = $"External exposure ({intakeUnit})";
            var ytitle = $"Internal exposure ({internalConcentrationUnit})";
            return createPlotModel(section, string.Empty, xtitle, ytitle);
        }

        protected override PlotModel createPlotModel(KineticModelSection section, string title, string xtitle, string ytitle) {
            var plotModel = base.createDefaultPlotModel(string.Empty);

            var minExternalExposures = section.ExternalExposures.Any(c => c > 0) ? section.ExternalExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
            var maxExternalExposures = section.ExternalExposures.Any(c => c > 0) ? section.ExternalExposures.Where(c => c > 0).Max() * 2 : 2;
            var minInternalExposures = double.MinValue;
            var maxInternalExposures = double.MaxValue;
            if (section.IsAcute) {
                minInternalExposures = section.PeakTargetExposures.Any(c => c > 0) ? section.PeakTargetExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
                maxInternalExposures = section.PeakTargetExposures.Any(c => c > 0) ? section.PeakTargetExposures.Where(c => c > 0).Max() * 1.1 : 2;
            } else {
                minInternalExposures = section.SteadyStateTargetExposures.Any(c => c > 0) ? section.SteadyStateTargetExposures.Where(c => c > 0).Min() * 0.1 : 0.1;
                maxInternalExposures = section.SteadyStateTargetExposures.Any(c => c > 0) ? section.SteadyStateTargetExposures.Where(c => c > 0).Max() * 1.1 : 2;
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

            if (section.IsAcute && section.PeakTargetExposures.Count == section.ExternalExposures.Count) {
                var data = new Collection<object>();
                for (int i = 0; i < section.ExternalExposures.Count; i++) {
                    data.Add(new {
                        PeakInternalValues = section.PeakTargetExposures[i],
                        ExternalValues = section.ExternalExposures[i]
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

                if (PlotLinearApproximationLine && !double.IsNaN(section.ConcentrationRatioPeak)) {
                    var lineSeriesAbsorptionPeak = new LineSeries() {
                        Color = OxyColors.Green,
                        LineStyle = LineStyle.Dash,
                        StrokeThickness = 1,
                    };
                    lineSeriesAbsorptionPeak.Points.Add(new DataPoint(minExternalExposures, minExternalExposures * section.ConcentrationRatioPeak));
                    lineSeriesAbsorptionPeak.Points.Add(new DataPoint(maxExternalExposures, maxExternalExposures * section.ConcentrationRatioPeak));
                    plotModel.Series.Add(lineSeriesAbsorptionPeak);
                }
            }

            if (!section.IsAcute && section.SteadyStateTargetExposures.Count == section.ExternalExposures.Count) {
                var data = new Collection<object>();
                for (int i = 0; i < section.ExternalExposures.Count; i++) {
                    data.Add(new {
                        MeanInternalValues = section.SteadyStateTargetExposures[i],
                        ExternalValues = section.ExternalExposures[i]
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

                if (PlotLinearApproximationLine && !double.IsNaN(section.ConcentrationRatioAverage)) {
                    var lineSeriesAbsorptionAverage = new LineSeries() {
                        Color = OxyColors.RoyalBlue,
                        LineStyle = LineStyle.Dash,
                        StrokeThickness = 1,
                    };
                    lineSeriesAbsorptionAverage.Points.Add(new DataPoint(minExternalExposures, minExternalExposures * section.ConcentrationRatioAverage));
                    lineSeriesAbsorptionAverage.Points.Add(new DataPoint(maxExternalExposures, maxExternalExposures * section.ConcentrationRatioAverage));
                    plotModel.Series.Add(lineSeriesAbsorptionAverage);
                }
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

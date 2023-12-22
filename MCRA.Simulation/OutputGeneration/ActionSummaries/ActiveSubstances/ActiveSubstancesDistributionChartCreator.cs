using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstancesDistributionChartCreator : ReportHistogramChartCreatorBase {

        private ActiveSubstancesSummarySection _section;

        public ActiveSubstancesDistributionChartCreator(ActiveSubstancesSummarySection section) {
            _section = section;
        }

        public override string Title { get; }

        public override string ChartId {
            get {
                var pictureId = "3035C61C-B146-4DE4-B69B-8F440FDE0D80";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.Records.SelectMany(r => r.MembershipProbabilities).ToList());
        }


        public PlotModel create(List<ActiveSubstanceRecord> records) {
            var values = records
                .Select(r => r.Probability)
                .Where(r => !double.IsNaN(r))
                .ToList();

            PlotModel plotModel = null;
            if (values.Any(r => r > 0 && r < 1)) {
                plotModel = createContinuous(values);
            } else {
                plotModel = createDiscrete(values);
            }
            return plotModel;
        }

        private PlotModel createContinuous(List<double> values) {
            var bins = HistogramBinUtilities.MakeHistogramBins(values, 0, 1);

            var title = "Assessment group memberships distribution";
            var plotModel = createDefaultPlotModel(title);

            var horizontalAxis = new LinearAxis() {
                Title = "Computed membership probability",
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = 1,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearVerticalAxis("Frequency", bins.Max(r => r.Frequency * 1.1));
            plotModel.Axes.Add(verticalAxis);

            var histogramSeries = createDefaultHistogramSeries(bins);
            plotModel.Series.Add(histogramSeries);
            return plotModel;
        }

        private PlotModel createDiscrete(List<double> values) {
            var title = "Assessment group memberships";
            var plotModel = createDefaultPlotModel(title);
            plotModel.PlotMargins = new OxyThickness(150, double.NaN, double.NaN, double.NaN);

            var positivesCount = values.Count(r => r >= .5);
            var negativesCount = values.Count(r => r < .5);
            var totalCount = values.Count;

            var barSeries = new BarSeries {
                ItemsSource = new List<BarItem>(new[] {
                    new BarItem{ Value = negativesCount },
                    new BarItem{ Value = positivesCount },
                }),
                FillColor = OxyColors.CornflowerBlue,
                StrokeColor = OxyColor.FromArgb(255, 78, 132, 233),
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0}"
            };
            plotModel.Series.Add(barSeries);

            plotModel.Axes.Add(new CategoryAxis {
                Position = AxisPosition.Left,
                Key = "Membership",
                ItemsSource = new[] {
                "Negative membership",
                "Positive membership"
                }
            });

            var horizontalAxis = new LinearAxis() {
                Title = "Number of substances",
                Position = AxisPosition.Bottom,
                Minimum = 0,
                MaximumPadding = 0.1,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(horizontalAxis);

            return plotModel;
        }
    }
}

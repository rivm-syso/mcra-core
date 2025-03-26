using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodChartCreator : ReportChartCreatorBase {

        private readonly List<AttributableBodSummaryRecord> _records;
        private readonly string _sectionId;
        private readonly string _bodIndicator;
        private readonly string _erfCode;
        private readonly string _unit;

        public AttributableBodChartCreator(
            List<AttributableBodSummaryRecord> records,
            string sectionId
        ) {
            Width = 500;
            Height = 350;
            _records = records;
            _sectionId = sectionId;
            _unit = records.First().Unit;
            _bodIndicator = records.First().BodIndicator;
            _erfCode = records.First().ExposureResponseFunctionCode;
        }

        public override string Title => "Attributable burden &amp; cumulative percentage.";
        public override string ChartId {
            get {
                var pictureId = "764e10fb-d6e0-4690-b5ce-ffdbd3643d5d";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _bodIndicator + _erfCode);
            }
        }

        public override PlotModel Create() {
            return create(_records);
        }

        private PlotModel create(
            List<AttributableBodSummaryRecord> records
        ) {
            var sum = records.Sum(c => c.AttributableBod) / 100;
            var plotModel = new PlotModel();
            var name = $"{_bodIndicator} - {_erfCode}";

            // Create bar series with error bars
            var errorBarSeries = new ErrorBarSeries() {
                FillColor = OxyColors.RoyalBlue,
                StrokeColor = OxyColors.RoyalBlue,
                ErrorStrokeThickness = 1.5,
                StrokeThickness = 1,
                XAxisKey = "x",
                YAxisKey = "y",
                Title = name
            };
            var error = 0d;
            foreach (var record in records) {
                errorBarSeries.Items.Add(new ErrorBarItem(record.AttributableBod, error));
            }
            plotModel.Series.Add(errorBarSeries);

            // Create line series
            var lineSeries = new LineSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 4,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Red,
                YAxisKey = "z"
            };
            var cumulative = 0d;
            for (var i = 0; i < records.Count; i++) {
                cumulative += records[i].AttributableBod;
                lineSeries.Points.Add(new DataPoint(i, cumulative / sum));
            }
            plotModel.Series.Add(lineSeries);

            // Add horizontal category axis
            var categoryAxis = new CategoryAxis() {
                Key = "y",
                MinorStep = 1,
                Title = $"Exposure bin ({_unit})",
            };
            foreach (var record in records) {
                categoryAxis.ActualLabels.Add(record.ExposureBin);
            }
            plotModel.Axes.Add(categoryAxis);

            // Left indicator axis
            var linearAxisLeft = new LinearAxis() {
                Key = "x",
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = $"Attributable Burden ({_bodIndicator})"
            };
            plotModel.Axes.Add(linearAxisLeft);

            // Right cumulative percentage axis
            var linearAxisRight = new LinearAxis() {
                TickStyle = TickStyle.Outside,
                Position = AxisPosition.Right,
                Minimum = 0,
                MaximumPadding = 0.02,
                MajorStep = 20,
                Key = "z",
                Title = $"Cumulative (%)",
            };
            plotModel.Axes.Add(linearAxisRight);

            // https://git.wur.nl/Biometris/mcra-dev/MCRA-Issues/-/issues/2172
            // This might be useful when multiple bars are plotted for e.g.
            // 1) the relative attributable BoD,
            // 2) the relative standardized attributable bod,
            // 3) the PAF (for top-down calculations, otherwise the proportion of excess cases), and
            // 4) the relative population size
            var legend = new Legend();
            legend.LegendOrientation = LegendOrientation.Horizontal;
            legend.LegendBorderThickness = 0;
            legend.LegendPlacement = LegendPlacement.Outside;
            legend.LegendPosition = LegendPosition.BottomCenter;
            //plotModel.Legends.Add(legend);
            return plotModel;
        }
    }
}

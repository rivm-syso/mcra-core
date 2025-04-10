using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodChartCreator : ReportChartCreatorBase {

        private readonly List<AttributableBodSummaryRecord> _records;
        private readonly string _sectionId;
        private readonly string _population;
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
            _unit = records.First().TargetUnit;
            _population = records.First().Population;
            _bodIndicator = records.First().BodIndicator;
            _erfCode = records.First().ExposureResponseFunctionCode;
        }

        public override string Title => "Attributable burden &amp; cumulative percentage.";
        public override string ChartId {
            get {
                var pictureId = "764e10fb-d6e0-4690-b5ce-ffdbd3643d5d";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _population + _bodIndicator + _erfCode);
            }
        }

        public override PlotModel Create() {
            return create(_records);
        }

        private PlotModel create(
            List<AttributableBodSummaryRecord> records
        ) {
            var uncertainty = records.SelectMany(c => c.AttributableBods).Any();
            var sumNominal = records.Sum(c => c.AttributableBod) / 100;
            var sumMedian = records.Sum(c => c.MedianAttributableBod) / 100;
            var plotModel = new PlotModel();
            var name = $"{_population} - {_bodIndicator} - {_erfCode}";

            // Create bar series with error bars
            var errorBarSeries = new ColumnWithErrorBarsSeries() {
                FillColor = OxyColors.RoyalBlue,
                StrokeColor = OxyColors.RoyalBlue,
                ErrorStrokeColor = OxyColors.Black,
                ErrorStrokeThickness = 1.5,
                StrokeThickness = 1,
                XAxisKey = "x",
                YAxisKey = "y",
                Title = name
            };
            foreach (var record in records) {
                errorBarSeries.Items.Add(
                    new ColumnWithErrorItem(
                        record.AttributableBod,
                        record.LowerAttributableBod,
                        record.UpperAttributableBod
                    )
                );
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


            // Add horizontal category axis
            var categoryAxis = new CategoryAxis() {
                Key = "y",
                MinorStep = 1,
                Title = $"Exposure bin ({_unit})",
                Angle = 45,
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
                Title = $"Attributable Burden ({_bodIndicator})",
                AbsoluteMinimum = 0
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

            if (uncertainty) {
                var areaSeries = new AreaSeries() {
                    Color = OxyColors.Red,
                    Color2 = OxyColors.Red,
                    Fill = OxyColor.FromAColor(50, OxyColors.Red),
                    StrokeThickness = .5,
                    YAxisKey = "z"
                };
                for (var i = 0; i < records.Count; i++) {
                    areaSeries.Points.Add(new DataPoint(i, records[i].LowerCumulativeAttributableBod));
                    areaSeries.Points2.Add(new DataPoint(i, records[i].UpperCumulativeAttributableBod));
                }
                plotModel.Series.Add(areaSeries);
            }
            for (var i = 0; i < records.Count; i++) {
                lineSeries.Points.Add(new DataPoint(i, records[i].CumulativeAttributableBod));
            }
            plotModel.Series.Add(lineSeries);

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

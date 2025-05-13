using MCRA.Utils.Charting.OxyPlot;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class AttributableBodChartCreatorBase : ReportChartCreatorBase {

        protected readonly List<AttributableBodSummaryRecord> _records;
        protected readonly string _sectionId;
        protected readonly string _population;
        protected readonly string _bodIndicator;
        protected readonly string _erfCode;
        protected readonly string _unit;

        public AttributableBodChartCreatorBase(
            List<AttributableBodSummaryRecord> records,
            string sectionId
        ) {
            Width = 370;
            Height = 300;
            _records = records;
            _sectionId = sectionId;
            _unit = records.First().TargetUnit;
            _population = records.First().PopulationName;
            _bodIndicator = records.First().BodIndicator;
            _erfCode = records.First().ErfCode;
        }

        protected static PlotModel create(
            List<(double Value, double Lower, double Upper)> bars,
            List<(double Value, double Lower, double Upper)> cumulative,
            List<string> labels,
            string unit,
            string leftYAxisTitle,
            bool uncertainty
        ) {
            var plotModel = new PlotModel();

            // Create bar series with error bars
            var errorBarSeries = new ColumnWithErrorBarsSeries() {
                FillColor = OxyColors.RoyalBlue,
                StrokeColor = OxyColors.RoyalBlue,
                ErrorStrokeColor = OxyColors.Black,
                ErrorStrokeThickness = 1.5,
                StrokeThickness = 1,
                XAxisKey = "x",
                YAxisKey = "y"
            };

            foreach (var (Value, Lower, Upper) in bars) {
                errorBarSeries.Items.Add(
                    new ColumnWithErrorItem(
                        Value,
                        Lower,
                        Upper
                    )
                );
            }
            plotModel.Series.Add(errorBarSeries);

            // Add horizontal category axis
            var categoryAxis = new CategoryAxis() {
                Key = "y",
                MinorStep = 1,
                Title = $"Exposure bin ({unit})",
                Angle = 45,
            };
            foreach (var label in labels) {
                categoryAxis.ActualLabels.Add(label);
            }
            plotModel.Axes.Add(categoryAxis);

            // Left indicator axis
            var linearAxisLeft = new LinearAxis() {
                Key = "x",
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = leftYAxisTitle,
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
            plotModel.Axes.Add(linearAxisRight);

            // Create line series
            var lineSeries = new LineSeries() {
                MarkerType = MarkerType.Circle,
                MarkerFill = OxyColors.Red,
                MarkerSize = 4,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Red,
                YAxisKey = "z"
            };
            for (var i = 0; i < cumulative.Count; i++) {
                lineSeries.Points.Add(new DataPoint(i, cumulative[i].Value));
            }
            plotModel.Series.Add(lineSeries);

            // Area series for visualisation of uncertainty of the line series
            if (uncertainty) {
                var areaSeries = new AreaSeries() {
                    Color = OxyColors.Red,
                    Color2 = OxyColors.Red,
                    Fill = OxyColor.FromAColor(50, OxyColors.Red),
                    StrokeThickness = .5,
                    YAxisKey = "z"
                };
                for (var i = 0; i < cumulative.Count; i++) {
                    areaSeries.Points.Add(new DataPoint(i, cumulative[i].Lower));
                    areaSeries.Points2.Add(new DataPoint(i, cumulative[i].Upper));
                }
                plotModel.Series.Add(areaSeries);
            }

            // https://git.wur.nl/Biometris/mcra-dev/MCRA-Issues/-/issues/2172
            // This might be useful when multiple bars are plotted for e.g.
            // 1) the relative attributable BoD,
            // 2) the relative standardised attributable bod,
            // 3) the PAF (for top-down calculations, otherwise the proportion of excess cases), and
            // 4) the relative population size
            var legend = new Legend {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter
            };
            //plotModel.Legends.Add(legend);
            return plotModel;
        }
    }
}

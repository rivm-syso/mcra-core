using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public class InternalConcentrationTimeSeriesChartCreator : ReportLineChartCreatorBase {

        public override string ChartId => "77ccf179-82a1-490f-a392-07fcb289c27b";

        private readonly InternalConcentrationTimeSeriesSection _section;
        private readonly string _compartment;

        public override string Title => $"Concentration {_compartment}.";

        public InternalConcentrationTimeSeriesChartCreator(
            InternalConcentrationTimeSeriesSection section,
            string compartment
        ) {
            Width = 250;
            Height = 250;
            _section = section;
            _compartment = compartment;
        }

        public override PlotModel Create() {
            var plotModel = createDefaultPlotModel();

            // Fetch the internal concentration series of the specified compartment
            var record = _section.Records.First(r => r.Compartment == _compartment);

            // Create line series
            var series = new LineSeries() {
                Color = OxyColors.Red,
                MarkerType = MarkerType.None,
                StrokeThickness = 1.5,
            };

            // Determine tick interval
            var maximumTickNumber = 11;
            var nDays = (int)Math.Ceiling(record.TimeScale.GetTimeUnitMultiplier(TimeUnit.Days) * record.Values.Length / record.TimeFrequency);
            var interval = Math.Ceiling(nDays / (double)maximumTickNumber);

            // Create horizontal (time) axis
            // Unit of time span axis is always seconds
            var timeSpanAxis = new TimeSpanAxis() {
                Position = AxisPosition.Bottom,
                StringFormat = "%d",
                Title = $"Time (d)",
                MajorStep = 3600 * 24 * interval,
                MinorStep = 3600 * 12 * interval,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(timeSpanAxis);

            // Vertical axis (concentration/amount)
            var concentrationAxis = new LinearAxis() {
                Position = AxisPosition.Left,
                Title = $"Concentration ({record.Unit.GetShortDisplayName()})"
            };
            plotModel.Axes.Add(concentrationAxis);

            // Compute scaling factor to convert to seconds
            var timeScalingFactor = record.TimeScale.GetTimeUnitMultiplier(TimeUnit.Seconds) / record.TimeFrequency;

            // Create time series
            for (int i = 0; i < record.Values.Length; i++) {
                var value = record.Values[i];
                series.Points.Add(new DataPoint(
                    timeScalingFactor * i,
                    value)
                );
            }
            plotModel.Series.Add(series);

            return plotModel;
        }
    }
}

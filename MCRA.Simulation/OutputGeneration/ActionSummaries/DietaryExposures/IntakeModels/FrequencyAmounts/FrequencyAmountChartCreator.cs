using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FrequencyAmountChartCreator : BoxPlotChartCreatorBase {

        private FrequencyAmountSummarySection _section;
        private string _intakeUnit;

        public FrequencyAmountChartCreator(FrequencyAmountSummarySection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "61fa2212-89fb-4e23-ab42-b522364ca65a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Frequencies versus amounts";

        public override PlotModel Create() {
            return create(_section, _intakeUnit);
        }

        private PlotModel create(FrequencyAmountSummarySection frequencyAmountSummarySection, string intakeUnit) {
            var plotModel = createDefaultPlotModel();
            LogarithmicAxis linearAxis2 = createLogarithmicLeftAxis(title: $"amount ({intakeUnit})");
            linearAxis2.MajorGridlineStyle = LineStyle.Dash;
            linearAxis2.MajorTickSize = 2;

            var categoryAxis1 = new CategoryAxis() {
                MinorStep = 1,
                Title = "frequency",
            };
            var series = new BoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, OxyColors.Red),
                StrokeThickness = 1,
                Stroke = OxyColors.Black,
                WhiskerWidth = 1,
            };

            var minimum = double.PositiveInfinity;
            var maximum = double.NegativeInfinity;
            var counter = 0;
            foreach (var item in frequencyAmountSummarySection.FrequencyAmountRelations) {
                if (item.NumberOfDays == 1) {
                    categoryAxis1.Labels.Add($"{item.NumberOfDays} day");
                } else {
                    categoryAxis1.Labels.Add($"{item.NumberOfDays} days");
                }
                series.Items.Add(new BoxPlotItem(counter, item.LowerWisker, item.LowerBox, item.Median, item.UpperBox, item.UpperWisker));
                minimum = Math.Min(minimum, item.LowerWisker);
                maximum = Math.Max(maximum, item.UpperWisker);
                counter++;
            };
            linearAxis2.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            plotModel.Axes.Add(linearAxis2);
            plotModel.Axes.Add(categoryAxis1);

            plotModel.Series.Add(series);
            return plotModel;
        }
    }
}



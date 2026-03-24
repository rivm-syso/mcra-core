using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureDistributionPercentileChartCreator : BoxPlotChartCreatorBase {

        private readonly ExposureDistributionPercentilesSection _section;
        private readonly string _intakeUnit;
        private readonly bool _stratified;
        private readonly string _title;
        private readonly string _category;
        private readonly int _paletteColor;
        private readonly int _numberOfStratifications;
        private readonly double _minimum;
        private readonly double _maximum;

        public ExposureDistributionPercentileChartCreator(
            ExposureDistributionPercentilesSection section,
            string intakeUnit,
            int paletteColor,
            double minimum,
            double maximum,
            int numberOfStratifications = 0,
            string category = null
        ) {
            Width = 350;
            Height = 300;
            _section = section;
            _intakeUnit = intakeUnit;
            _category = category;
            _title = $"Uncertainty of percentiles: {category}";
            _paletteColor = paletteColor;
            _numberOfStratifications = numberOfStratifications;
            _minimum = minimum;
            _maximum = maximum;
        }

        public override string ChartId {
            get {
                var pictureId = "02022199-6e33-48e6-b109-81d1f37d6cfd" + _category;
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => _title;

        public override PlotModel Create() {
            var palette = CustomPalettes.DietaryNonDietaryColors(_numberOfStratifications);
            var records = _section.Records.Where(c => c.Stratification == _category).ToList();
            var color = _numberOfStratifications == 0 ? OxyColor.FromAColor(100, OxyColors.RoyalBlue) : palette.Colors[_paletteColor];
            return create(records, color);
        }

        private PlotModel create(List<TargetExposurePercentileRecord> records, OxyColor color) {
            var uncertaintyLowerBound = _section.UncertaintyLowerLimit;
            var uncertaintyUpperBound = _section.UncertaintyUpperLimit;
            var stratifications = records.Select(c => c.Stratification).ToHashSet();
            var plotModel = createDefaultPlotModel();
            var linearAxis2 = createLinearLeftAxis($"Exposure ({_intakeUnit})");
            linearAxis2.MajorGridlineStyle = LineStyle.Dash;
            linearAxis2.MajorTickSize = 2;
            linearAxis2.Maximum = _maximum;
            linearAxis2.Minimum = _minimum;

            var categoryAxis1 = new CategoryAxis() {
                MinorStep = 1,
                Title = "Percentage (%)",
            };

            var series = new BoxPlotSeries() {
                Fill = color,
                StrokeThickness = 1,
                Stroke = OxyColors.Black,
                WhiskerWidth = 1,
            };

            var counter = 0;
            foreach (var item in records) {
                var dp = asBoxPlotDataPoint(item.Values, lowerBound: uncertaintyLowerBound, upperBound: uncertaintyUpperBound);
                categoryAxis1.Labels.Add($"p{item.XValue * 100:F2}");
                var boxPlotItem = new BoxPlotItem(counter, dp.LowerWisker, dp.LowerBox, dp.Median, dp.UpperBox, dp.UpperWisker) {
                    Outliers = dp.Outliers,
                    Mean = item.Values.Average(),
                };
                series.Items.Add(boxPlotItem);
                counter++;
            }
            linearAxis2.MajorStep = double.NaN;
            plotModel.Axes.Add(linearAxis2);
            plotModel.Axes.Add(categoryAxis1);
            plotModel.Series.Add(series);
            return plotModel;
        }
    }
}



using MCRA.General;
using MCRA.Utils;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ConsumerProductConcentrationModelChartCreatorBase : ReportHistogramChartCreatorBase {

        protected ConsumerProductConcentrationModelRecord _record;
        protected bool _showTitle;

        public ConsumerProductConcentrationModelChartCreatorBase(
            ConsumerProductConcentrationModelRecord concentrationModelRecord,
            int height,
            int width,
            bool showTitle
        ) {
            Height = height;
            Width = width;
            _record = concentrationModelRecord;
            _showTitle = showTitle;
        }

        public override abstract string ChartId { get; }

        protected PlotModel createCensoredValuesBarSeries(ConsumerProductConcentrationModelRecord record) {
            var horizontalMargin = (Width - 100) / 2D;
            var model = new PlotModel {
                Title = "No positives",
                TitleFontSize = 11,
                TitleFontWeight = 200,
                PlotMargins = new OxyThickness(horizontalMargin, double.NaN, horizontalMargin, 25),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var histogramSeriesTrueZero = new OxyPlot.Series.HistogramSeries {
                LabelPlacement = LabelPlacement.Outside,
                FontSize = 10,
                LabelFormatString = "{0:.##}%",
                StrokeThickness = 1,
                LabelMargin = 1
            };
            var histogramSeriesCensored = new OxyPlot.Series.HistogramSeries {
                LabelPlacement = LabelPlacement.Outside,
                FontSize = 10,
                LabelFormatString = "{0:.##}%",
                StrokeThickness = 1,
                LabelMargin = 1
            };

            var barWidth = 0.8;
            var horizontalAxisMinValue = -0.5;
            var horizontalAxisMaxValue = 1.5;
            var valueTrueZero = record.FractionTrueZeros * 100;   // = height of bar
            var areaTrueZero = valueTrueZero * barWidth;

            var valueCensored = record.FractionCensored * 100;   // = height of bar
            var areaCensored = valueCensored * barWidth;

            histogramSeriesTrueZero.Items.Add(new HistogramItem(-0.4, -0.4 + barWidth, areaTrueZero, 3, OxyColors.LimeGreen));
            histogramSeriesCensored.Items.Add(new HistogramItem(0.6, 0.6 + barWidth, areaCensored, 3, OxyColors.Red));

            var verticalAxis = new LinearAxis() {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 150,
                IsAxisVisible = false,
            };

            var categoryAxisBottom = new CategoryAxis {
                Minimum = horizontalAxisMinValue,
                Maximum = horizontalAxisMaxValue,
                Angle = -90,
                AxislineStyle = LineStyle.Solid,
                FontSize = 9
            };
            categoryAxisBottom.Labels.AddRange(["Zero", "Cens"]);

            model.Axes.Add(verticalAxis);
            model.Axes.Add(categoryAxisBottom);
            model.Series.Add(histogramSeriesTrueZero);
            model.Series.Add(histogramSeriesCensored);

            return model;
        }

        /// <summary>
        /// Exposures are natural logarithm transformed, mu and sigma are calculated on the natural logarithm scale
        /// The plot is on the log10 scale
        /// </summary>
        /// <param name="record"></param>
        /// <param name="showTitle"></param>
        /// <returns></returns>
        protected PlotModel create(
            ConsumerProductConcentrationModelRecord record,
            bool showTitle,
            bool showCensoredValueBins,
            bool showCensoredValueBars) {
            var plotModel = new PlotModel();
            plotModel.PlotMargins = new OxyThickness(showCensoredValueBars ? 60 : 20, 0, 0, 20
        );

            if (showTitle) {
                plotModel.Title = record.Model.GetDisplayAttribute().ShortName;
                plotModel.ClipTitle = false;
                plotModel.TitleFontSize = 11;
                plotModel.TitleFontWeight = 200;
            }

            var logarithmicAxis = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Base = 10,
                UseSuperExponentialFormat = false,
                MajorTickSize = 4,
                MajorGridlineStyle = LineStyle.Dash,
                FontSize = 9,
                Angle = 45,
                TitleFontWeight = 200,
            };
            plotModel.Axes.Add(logarithmicAxis);

            var verticalAxis = new LinearAxis() {
                Position = AxisPosition.Left,
                FontSize = 9,
                Minimum = 0,
            };
            plotModel.Axes.Add(verticalAxis);

            var minimumX = 0.01;
            var maximumX = 1d;
            var maximumY = 1d;
            var totalArea = 1d;
            var alignmentPoints = new List<double>();

            List<HistogramBin> histogramBins = null;
            if (record.LogPositiveResiduesBins != null
                && record.LogPositiveResiduesBins.Any()) {

                var logHistogramBins = record.LogPositiveResiduesBins
                    .Where(r => !double.IsNaN(r.XMidPointValue) && !double.IsInfinity(r.XMidPointValue))
                    .ToList();

                histogramBins = logHistogramBins.Select(r => new HistogramBin() {
                    Frequency = r.Frequency,
                    XMinValue = Math.Exp(r.XMinValue),
                    XMaxValue = Math.Exp(r.XMaxValue),
                }).ToList();

                var histogramSeries = createDefaultHistogramSeries(histogramBins);
                plotModel.Series.Add(histogramSeries);

                minimumX = histogramBins.GetMinBound();
                maximumX = histogramBins.GetMaxBound() + histogramBins.AverageBinSize();
                maximumY = histogramBins.Select(c => c.Frequency).Max() * 1.1;
                totalArea = logHistogramBins.Sum(b => b.Width * b.Frequency);
            }

            // Create the censored value area
            if (showCensoredValueBins && record.LORs.Any()) {
                var maxLor = (histogramBins != null && histogramBins.Any()) ? histogramBins.First().XMinValue : record.LORs.Max();
                var logLOR = Math.Log10(maxLor);
                var fractionCensoredValues = record.FractionCensored + record.FractionTrueZeros;
                var censoredNonDetectsCount = record.FractionCensored * record.CensoredValuesCount / fractionCensoredValues;

                var censoredBins = 1d;
                var maxFrequency = histogramBins.Max(b => b.Frequency);
                var binWidth = histogramBins.First().Width;
                while ((record.CensoredValuesCount / censoredBins) > 10 * maxFrequency
                    || (maxLor - censoredBins * binWidth) > binWidth) {
                    censoredBins += 1.0;
                }
                var censoredValueBinHeight = record.CensoredValuesCount / censoredBins;

                var fractionCensored = record.FractionCensored / fractionCensoredValues;
                var fractionTrueZero = record.FractionTrueZeros / fractionCensoredValues;
                var pCensoredValueMin = Math.Pow(10, logLOR - censoredBins * binWidth);
                var pZeroUpper = Math.Pow(10, logLOR - (1 - fractionTrueZero) * censoredBins * binWidth);
                var pCensoredLower = Math.Pow(10, logLOR - fractionCensored * censoredBins * binWidth); ;

                var trueZeroSeries = new AreaSeries() {
                    Color = OxyColor.FromAColor(100, OxyColors.LimeGreen),
                    StrokeThickness = 2,
                    MarkerStroke = OxyColors.LimeGreen,
                };
                trueZeroSeries.Points.Add(new DataPoint(pCensoredValueMin, censoredValueBinHeight));
                trueZeroSeries.Points.Add(new DataPoint(pZeroUpper, censoredValueBinHeight));
                plotModel.Series.Add(trueZeroSeries);

                var censoredSeries = new AreaSeries() {
                    Color = OxyColor.FromAColor(100, OxyColors.Red),
                    StrokeThickness = 2,
                    MarkerStroke = OxyColors.Red,
                };
                censoredSeries.Points.Add(new DataPoint(pCensoredLower, censoredValueBinHeight));
                censoredSeries.Points.Add(new DataPoint(maxLor, censoredValueBinHeight));
                plotModel.Series.Add(censoredSeries);

                minimumX = Math.Min(minimumX, pCensoredValueMin);
                maximumX = Math.Max(maximumX, maxLor);
            }

            if (record.Model == ConcentrationModelType.MaximumResidueLimit) {
                var mrl = record.MaximumResidueLimit.Value;
                var factor = record.FractionOfMrl ?? 1d;

                minimumX = Math.Min(minimumX, .9 * factor * mrl);
                maximumX = Math.Max(maximumX, 1.1 * mrl);

                logarithmicAxis.Minimum = minimumX;
                logarithmicAxis.Maximum = maximumX;
                verticalAxis.Maximum = maximumY;

                var mrlLineAnnotation = new LineAnnotation() {
                    Type = LineAnnotationType.Vertical,
                    X = mrl,
                    Color = OxyColors.OrangeRed,
                    StrokeThickness = 1,
                    LineStyle = LineStyle.Dash
                };
                plotModel.Annotations.Add(mrlLineAnnotation);

                var factorMrlLineAnnotation = new LineAnnotation() {
                    Type = LineAnnotationType.Vertical,
                    X = factor * mrl,
                    Color = OxyColors.Green,
                    StrokeThickness = 3,
                    LineStyle = LineStyle.Solid
                };

                plotModel.Annotations.Add(factorMrlLineAnnotation);
            }

            // Plot the fit
            if (record.Model != ConcentrationModelType.Empirical
                && record.Model != ConcentrationModelType.MaximumResidueLimit
                && record.Mu.HasValue && !double.IsNaN(record.Mu.Value)
                && record.Sigma.HasValue && !double.IsNaN(record.Sigma.Value)
                && record.Sigma.Value > 0
            ) {

                var logMaximum = Math.Log(maximumX);
                var logMinimum = Math.Log(minimumX);
                var mu = record.Mu.Value;
                var sigma = record.Sigma.Value;

                if (double.IsNaN(maximumX) || logMaximum < mu + 2 * sigma) {
                    maximumX = Math.Exp(mu + 2 * sigma);
                    logMaximum = Math.Log(maximumX);
                    logarithmicAxis.Maximum = maximumX;
                }
                if (double.IsNaN(minimumX) || logMinimum > mu - 2 * sigma) {
                    minimumX = Math.Exp(mu - 2 * sigma);
                    logMinimum = Math.Log(minimumX);
                    logarithmicAxis.Minimum = minimumX;
                }

                var fitSeries = new LineSeries() {
                    Color = OxyColors.Black,
                    XAxisKey = "normalDensity",
                    StrokeThickness = 0.8,
                };
                var normalDensityAxis = new LinearAxis() {
                    Key = "normalDensity",
                    Position = AxisPosition.Top,
                    IsAxisVisible = false,
                    Minimum = logMinimum,
                    Maximum = logMaximum,
                };
                plotModel.Axes.Add(normalDensityAxis);

                if (record.Model == ConcentrationModelType.CensoredLogNormal
                    || record.Model == ConcentrationModelType.ZeroSpikeCensoredLogNormal) {
                    // Check to see if the fraction of true zeros is NaN (might happen fo cumulative models)
                    var fts = !double.IsNaN(record.FractionTrueZeros) ? record.FractionTrueZeros : 0;
                    totalArea = (1 - fts) * totalArea / record.FractionPositives;
                }

                var normalDensity = GriddingFunctions.Arange(logMinimum, logMaximum, 500)
                    .Select(v => (x: v, y: NormalDistribution.PDF(mu, sigma, v) * totalArea))
                    .ToList();

                foreach (var item in normalDensity) {
                    fitSeries.Points.Add(new DataPoint(item.x, item.y));
                }
                plotModel.Series.Add(fitSeries);

                maximumY = Math.Max(maximumY, normalDensity.Select(c => c.y).Max() * 1.1);
            }

            if (showCensoredValueBars && record.CensoredValuesCount + record.FractionTrueZeros > 0) {
                plotModel = createCensoredValueBarsAxis(plotModel, record.FractionCensored, record.FractionTrueZeros);
            }

            logarithmicAxis.Minimum = minimumX;
            logarithmicAxis.Maximum = maximumX;
            logarithmicAxis.MajorStep = getSmartInterval(minimumX, maximumX, 4);
            verticalAxis.Maximum = maximumY;
            verticalAxis.MajorStep = getSmartInterval(0, maximumY, 4);

            return plotModel;
        }

        protected PlotModel createCensoredValueBarsAxis(
            PlotModel plotModel,
            double fractionCensored,
            double fractionTrueZero
        ) {
            var censBar = new NonDetectBarsAxis() {
                Fraction = !double.IsNaN(fractionCensored) ? fractionCensored : 0D,
                Label = "Cens",
                AxisDistance = 20,
                Height = Height,
                FontSize = 9,
            };
            plotModel.Axes.Add(censBar);
            var zeroBar = new NonDetectBarsAxis() {
                Fraction = !double.IsNaN(fractionTrueZero) ? fractionTrueZero : 0D,
                Label = "Zero",
                AxisDistance = 40,
                Color = OxyColors.LimeGreen,
                Height = Height,
                FontSize = 9,
            };
            plotModel.Axes.Add(zeroBar);
            return plotModel;
        }
    }
}

﻿using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RiskRatioPercentileChartCreator : BoxPlotChartCreatorBase {

        private readonly RiskRatioPercentileSection _section;

        public RiskRatioPercentileChartCreator(RiskRatioPercentileSection section) {
            Width = 500;
            Height = 350;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "d63037d8-e99d-4402-ad55-c1f8db0e9c86";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Uncertainty of percentiles." +
            $"The boxplots for uncertainty show the p25 and p75 as edges of the box, " +
            $"and p{_section.UncertaintyLowerLimit} and p{_section.UncertaintyUpperLimit} as edges of the whiskers. " +
            $"The reference value is indicated with the dashed black line, the median with the solid black line within the box. " +
            $"Outliers are displayed as dots outside the wiskers.";

        public override PlotModel Create() {
            return create(_section);
        }

        private PlotModel create(RiskRatioPercentileSection section) {
            var plotModel = createDefaultPlotModel();

            var linearAxis2 = createLinearLeftAxis($"Risk characterisation ratio");
            linearAxis2.MajorGridlineStyle = LineStyle.Dash;
            linearAxis2.MajorTickSize = 2;

            var categoryAxis1 = new CategoryAxis() {
                MinorStep = 1,
                Title = "percentage (%)",
            };
            var series = new BoxPlotSeries() {
                Fill = OxyColor.FromAColor(100, OxyColors.RoyalBlue),
                StrokeThickness = 1,
                Stroke = OxyColors.Black,
                WhiskerWidth = 1,
            };

            var minimum = double.PositiveInfinity;
            var maximum = double.NegativeInfinity;
            var counter = 0;
            foreach (var item in section.Percentiles) {
                var dp = asBoxPlotDataPoint(item);
                categoryAxis1.Labels.Add($"p{item.XValue:F2}");
                var boxPlotItem = new BoxPlotItem(counter, dp.LowerWisker, dp.LowerBox, dp.Median , dp.UpperBox, dp.UpperWisker) {
                    Outliers = dp.Outliers,
                    Mean =item.ReferenceValue,
                };
                series.Items.Add(boxPlotItem);
                minimum = Math.Min(minimum, dp.LowerWisker);
                maximum = Math.Max(maximum, dp.UpperWisker);
                counter++;
            };
            linearAxis2.MajorStep = Math.Pow(10, Math.Ceiling(Math.Log10((maximum - minimum) / 5)));
            linearAxis2.MajorStep = linearAxis2.MajorStep > 0 ? linearAxis2.MajorStep : double.NaN;
            plotModel.Axes.Add(linearAxis2);
            plotModel.Axes.Add(categoryAxis1);

            plotModel.Series.Add(series);
            return plotModel;
        }
    }
}



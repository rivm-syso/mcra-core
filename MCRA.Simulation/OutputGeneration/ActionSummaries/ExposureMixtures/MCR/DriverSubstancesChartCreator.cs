using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Modelling;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DriverSubstancesChartCreator : DriverCompoundsChartCreatorBase {

        private MaximumCumulativeRatioSection _section;
        private double? _percentage;
        private string _title;
        private string _xTitle;
        private string _definition;

        public DriverSubstancesChartCreator(MaximumCumulativeRatioSection section, double? percentage = null) {
            Height = 400;
            Width = 500;
            _section = section;
            _percentage = percentage;
            _title = _percentage == null ? "(total)" : $"(upper tail {_percentage}%)";
            _definition = _section.IsRiskMcrPlot ? "risk" : "exposure";
            var unit = _section.TargetUnit?.GetShortDisplayName() ?? string.Empty;
            _xTitle = _section.IsRiskMcrPlot
                ? (_section.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted ? $"Cumulative exposure ({unit})" : "Risk characterisation ratio (E/H)")
                : $"Cumulative exposure ({_section.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)})";
        }

        public override string ChartId {
            get {
                var pictureId = "aaca4f63-9d38-448c-96d2-11373a3931e4";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _percentage);
            }
        }
        public override string Title => $"Using MCR to identify substances that drive cumulative {_definition}, scatter distributions {_title}.";

        public override PlotModel Create() {
            var xTitle = _xTitle;
            return create(
                _section.DriverSubstanceTargets,
                _section.RatioCutOff,
                _section.Percentiles,
                _section.CumulativeExposureCutOffPercentage,
                _section.MinimumPercentage,
                _section.Threshold,
                xTitle
            );
        }

        private PlotModel create(
            List<DriverSubstanceRecord> drivers,
            double ratioCutOff,
            double[] percentiles,
            double totalExposureCutOff,
            double minimumPercentage,
            double threshold,
            string xTitle
        ) {
            var (plotModel, selectedDrivers, pExposure, maximumNumberPalette) = createMCRChart(
                drivers,
                ratioCutOff,
                percentiles,
                totalExposureCutOff,
                minimumPercentage,
                _percentage,
                threshold,
                xTitle
            );

            for (int p = 0; p < pExposure.Length; p++) {
                var subsetDrivers = drivers
                    .Where(c => c.CumulativeExposure > pExposure[p])
                    .ToList();
                if (subsetDrivers.Count > 1) {
                    var logCumulativeExposures = subsetDrivers
                        .Select(c => Math.Log(c.CumulativeExposure))
                        .ToList();
                    var logRatios = subsetDrivers
                        .Select(c => c.Ratio)
                        .ToList();
                    var minimumExposureSubset = logCumulativeExposures.Min();
                    var maximumExposureSubset = logCumulativeExposures.Max();
                    var modelResult = SimpleLinearRegressionCalculator.Compute(logCumulativeExposures, logRatios);
                    var steps = 10;
                    var delta = (maximumExposureSubset - minimumExposureSubset) / steps;
                    var fittedValues = new List<double>();
                    var xValues = new List<double>();
                    var x = minimumExposureSubset;
                    for (int i = 0; i < steps + 1; i++) {
                        xValues.Add(Math.Exp(x));
                        fittedValues.Add(modelResult.Constant + modelResult.Coefficient * x);
                        x += delta;
                    }
                    var fitLineSeries = new LineSeries() {
                        Color = OxyColors.Black,
                        MarkerType = MarkerType.None,
                    };
                    for (int i = 0; i < fittedValues.Count; i++) {
                        fitLineSeries.Points.Add(new DataPoint(xValues[i], fittedValues[i]));
                    }
                    plotModel.Series.Add(fitLineSeries);
                }
            }
            return plotModel;
        }
    }
}

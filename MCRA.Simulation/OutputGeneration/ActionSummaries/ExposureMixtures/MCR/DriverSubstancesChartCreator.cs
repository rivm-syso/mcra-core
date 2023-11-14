using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DriverSubstancesChartCreator : DriverCompoundsChartCreatorBase {

        private MaximumCumulativeRatioSection _section;
        private double? _percentage;
        private string _title;

        public DriverSubstancesChartCreator(MaximumCumulativeRatioSection section, double? percentage = null) {
            Height = 400;
            Width = 500;
            _section = section;
            _percentage = percentage;
            _title = _percentage == null ? "(total)" : $"(upper tail {_percentage}%)";
        }

        public override string ChartId {
            get {
                var pictureId = "aaca4f63-9d38-448c-96d2-11373a3931e4";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _percentage);
            }
        }
        public override string Title => $"Using MCR to identify substances that drive cumulative exposures, scatter distributions {_title}.";

        public override PlotModel Create() {
            return create(
                _section.DriverCompounds,
                _section.RatioCutOff,
                _section.Percentiles,
                _section.CumulativeExposureCutOffPercentage,
                _section.MinimumPercentage,
                _section.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)
            );
        }

        private PlotModel create(
            List<DriverCompoundRecord> drivers,
            double ratioCutOff,
            double[] percentiles,
            double totalExposureCutOff,
            double minimumPercentage,
            string intakeUnit
        ) {
            var (plotModel, selectedSubstances, pExposure) = createMCRChart(drivers,
                 ratioCutOff,
                 percentiles,
                 totalExposureCutOff,
                 minimumPercentage,
                 _percentage,
                 intakeUnit
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

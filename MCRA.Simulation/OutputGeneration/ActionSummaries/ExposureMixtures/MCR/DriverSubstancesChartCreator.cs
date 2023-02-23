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
                _section.TargetUnit.GetShortDisplayName(true)
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
            var minimumExposure = double.NaN;
            if (_percentage != null) {
                minimumExposure = drivers
                    .Select(c => c.CumulativeExposure)
                    .Percentile((double)_percentage);
            }

            var totalExposure = drivers.Sum(c => c.CumulativeExposure);
            var substances = drivers
                .GroupBy(c => c.CompoundCode)
                .Select(c => (
                    SubstanceCode: c.Key,
                    SubstanceName: c.First().CompoundName,
                    ExposureContribution: c.Sum(r => r.CumulativeExposure) / totalExposure * 100
                )).ToList();

            var selectedSubstances = substances
                .Where(c => c.ExposureContribution > minimumPercentage)
                .Select(c => (
                    c.SubstanceCode,
                    c.SubstanceName,
                    c.ExposureContribution
                ))
                .ToList();

            var selectedDrivers = drivers
                .Where(c => selectedSubstances.Select(s => s.SubstanceCode).Contains(c.CompoundCode))
                .Select(c => c)
                .ToList();

            if (selectedSubstances.Count == 0) {
                selectedDrivers = drivers;
                selectedSubstances = substances;
            }

            var cumulativeExposures = drivers
                .Select(c => c.CumulativeExposure)
                .ToList();
            var ratios = drivers
                .Select(c => c.Ratio)
                .ToList();

            if (percentiles.Length == 0) {
                percentiles = new double[1] { 50 };
            }
            var pExposure = cumulativeExposures.Percentiles(percentiles);
            var pRatio = ratios.Percentiles(percentiles);
            minimumExposure = double.IsNaN(minimumExposure) ? cumulativeExposures.Min() : minimumExposure;
            var maximumExposure = cumulativeExposures.Max();
            var ratioMax = ratios.Max();
            var ratioMin = ratios.Min();

            var plotModel = base.createPlotModel(string.Empty);
            var logarithmicAxis1 = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = $"Cumulative exposure ({intakeUnit})",
                Minimum = minimumExposure,
                Maximum = maximumExposure,
            };
            plotModel.Axes.Add(logarithmicAxis1);

            var linearAxis2 = new LinearAxis() {
                Title = "Maximum Cumulative Ratio",
                Minimum = 1,
                Maximum = ratioMax,
            };
            plotModel.Axes.Add(linearAxis2);

            var basePalette = OxyPalettes.Rainbow(selectedSubstances.Count == 1 ? 2 : selectedSubstances.Count);
            var counter = 0;
            var selectedSubstanceName = selectedDrivers
                .GroupBy(c => c.CompoundCode)
                .Select(c => (
                    SubstanceName: c.First().CompoundName,
                    N: c.Count()
                ))
                .OrderByDescending(c => c.N)
                .Select(c => c.SubstanceName)
                .ToList();

            foreach (var name in selectedSubstanceName) {
                var scatterSeries = new ScatterSeries() {
                    MarkerSize = 2,
                    MarkerType = MarkerType.Circle,
                    Title = name,
                    MarkerFill = basePalette.Colors.ElementAt(counter),
                };
                var set = selectedDrivers.Where(c => c.CompoundName == name).Select(c => c).ToList();
                for (int i = 0; i < set.Count; i++) {
                    scatterSeries.Points.Add(new ScatterPoint(set[i].CumulativeExposure, set[i].Ratio));
                }
                plotModel.Series.Add(scatterSeries);
                counter++;
            }

            var tUp = new List<string>();
            foreach (var item in percentiles) {
                tUp.Add($"p{item}");
            }
            for (int i = 0; i < pExposure.Length; i++) {
                var lineSeries = createLineSeries(OxyColors.Black);
                lineSeries.Points.Add(new DataPoint(pExposure[i], ratioMin));
                lineSeries.Points.Add(new DataPoint(pExposure[i], ratioMax));
                plotModel.Series.Add(lineSeries);

                var lineAnnotation = createLineAnnotation(ratioMax * .95, tUp[i]);
                lineAnnotation.MaximumX = pExposure[i] == logarithmicAxis1.Minimum ? pExposure[i] * 1.1 : pExposure[i];
                plotModel.Annotations.Add(lineAnnotation);
            }

            if (ratioCutOff > 0 && totalExposureCutOff == 0) {
                var lineSeries = createLineSeries(OxyColors.Gray);
                lineSeries.Points.Add(new DataPoint(minimumExposure, ratioCutOff));
                lineSeries.Points.Add(new DataPoint(maximumExposure, ratioCutOff));
                plotModel.Series.Add(lineSeries);
            }

            for (int p = 0; p < pExposure.Length; p++) {
                var subsetDrivers = drivers
                    .Where(c => c.CumulativeExposure > pExposure[p])
                    .Select(c => c)
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

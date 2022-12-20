using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    /// <summary>
    /// Is replaced by DriverSubstancesChartCreator
    /// </summary>
    public sealed class DriverCompoundsChartCreator : DriverCompoundsChartCreatorBase {

        private MaximumCumulativeRatioSection _section;

        public DriverCompoundsChartCreator(MaximumCumulativeRatioSection section) {
            Height = 400;
            Width = 500;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "94f33342-4ae4-4777-bc84-633540795cd4";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Using MCR to identify substances that drive cumulative exposures, scatter distributions.";

        public override PlotModel Create() {
            var compoundNames = _section.DriverCompoundStatisticsRecords
                .Select(c => c.CompoundName)
                .ToList();

            return create(
                _section.DriverCompounds,
                compoundNames,
                _section.RatioCutOff,
                _section.Percentiles,
                _section.CumulativeExposureCutOffPercentage,
                _section.TargetUnit.GetShortDisplayName(true)
             );
        }

        private PlotModel create(
                List<DriverCompoundRecord> drivers,
                List<string> compoundNames,
                double ratioCutOff,
                double[] percentiles,
                double totalExposureCutOffPercentage,
                string intakeUnit
            ) {
            var cumulativeExposure = drivers.Select(c => c.CumulativeExposure).ToList();
            if (percentiles == null || percentiles.Length == 0) {
                percentiles = new double[1] { 50 };
            }
            var pExposure = cumulativeExposure.Percentiles(percentiles);
            var ratio = drivers.Select(c => c.Ratio).ToList();
            var pRatio = ratio.Percentiles(percentiles);

            //set p95 per totalexposure section
            var percentage = 95;
            var minimum = cumulativeExposure.Min();
            var pRatioPerExposure = new List<double>();
            var pLeftBoundExposure = new List<double>();
            var pRightBoundExposure = new List<double>();
            for (int i = 0; i < pExposure.Length; i++) {
                pLeftBoundExposure.Add(minimum);
                pRightBoundExposure.Add(pExposure[i]);
                var selection = drivers.Where(c => c.CumulativeExposure > minimum && c.CumulativeExposure <= pExposure[i]).Select(c => c.Ratio).ToList();
                if (selection.Count > 0) {
                    pRatioPerExposure.Add(selection.Percentile(percentage));
                } else {
                    pRatioPerExposure.Add(1);
                }
                minimum = pExposure[i];
            }
            pRatioPerExposure.Add(drivers.Where(c => c.CumulativeExposure > pExposure.Last()).Select(c => c.Ratio).ToList().Percentile(percentage));
            pLeftBoundExposure.Add(minimum);
            pRightBoundExposure.Add(cumulativeExposure.Max());

            var plotModel = base.createPlotModel(string.Empty);
            //var plotModel = base.createPlotModel(String.Format("Using MCR to identify substances that drive cumulative exposures"));

            var logarithmicAxis1 = new LogarithmicAxis() {
                Position = AxisPosition.Bottom,
                Title = $"Cumulative exposure ({intakeUnit})",
                Minimum = cumulativeExposure.Min(),
                Maximum = cumulativeExposure.Max(),
            };
            plotModel.Axes.Add(logarithmicAxis1);

            var linearAxis2 = new LinearAxis() {
                Title = "Maximum Cumulative Ratio",
                Minimum = 1,
                Maximum = ratio.Max(),
            };
            plotModel.Axes.Add(linearAxis2);

            var basePalette = OxyPalettes.Rainbow(compoundNames.Count == 1 ? 2 : compoundNames.Count);
            var counter = 0;
            foreach (var name in compoundNames) {
                var scatterSeries = new ScatterSeries() {
                    MarkerSize = 2,
                    MarkerType = MarkerType.Circle,
                    Title = name,
                    MarkerFill = basePalette.Colors.ElementAt(counter),
                };
                var set = drivers.Where(c => c.CompoundName == name).Select(c => c).ToList();
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
                lineSeries.Points.Add(new DataPoint(pExposure[i], 1));
                lineSeries.Points.Add(new DataPoint(pExposure[i], ratio.Max()));
                plotModel.Series.Add(lineSeries);

                var lineAnnotation = createLineAnnotation(ratio.Max() * .95, tUp[i]);
                lineAnnotation.MaximumX = pExposure[i] == logarithmicAxis1.Minimum ? pExposure[i] * 1.1 : pExposure[i];
                plotModel.Annotations.Add(lineAnnotation);
            }

            for (int i = 0; i < pRatio.Length; i++) {
                var lineSeries = createLineSeries(OxyColors.Black);
                lineSeries.Points.Add(new DataPoint(cumulativeExposure.Max(), pRatio[i]));
                lineSeries.Points.Add(new DataPoint(cumulativeExposure.Min(), pRatio[i]));
                plotModel.Series.Add(lineSeries);
                var lineAnnotation = createLineAnnotation(pRatio[i], tUp[i]);
                plotModel.Annotations.Add(lineAnnotation);
            }

            if (ratioCutOff > 0 && totalExposureCutOffPercentage == 0) {
                var lineSeries = createLineSeries(OxyColors.Red);
                lineSeries.Points.Add(new DataPoint(cumulativeExposure.Min(), ratioCutOff));
                lineSeries.Points.Add(new DataPoint(cumulativeExposure.Max(), ratioCutOff));
                plotModel.Series.Add(lineSeries);
            }

            if (ratioCutOff == 0 && totalExposureCutOffPercentage > 0) {
                var co = cumulativeExposure.Percentile(totalExposureCutOffPercentage);
                var lineSeries = createLineSeries(OxyColors.Red);
                lineSeries.Points.Add(new DataPoint(co, 1));
                lineSeries.Points.Add(new DataPoint(co, ratio.Max()));
                plotModel.Series.Add(lineSeries);
            }

            if (ratioCutOff > 0 && totalExposureCutOffPercentage > 0) {
                var co = cumulativeExposure.Percentile(totalExposureCutOffPercentage);
                var lineSeries = createLineSeries(OxyColors.Red);
                lineSeries.Points.Add(new DataPoint(co, ratioCutOff));
                lineSeries.Points.Add(new DataPoint(co, ratio.Max()));
                plotModel.Series.Add(lineSeries);
                lineSeries = createLineSeries(OxyColors.Red);
                lineSeries.Points.Add(new DataPoint(co, ratioCutOff));
                lineSeries.Points.Add(new DataPoint(cumulativeExposure.Max(), ratioCutOff));
                plotModel.Series.Add(lineSeries);
            }

            for (int i = 0; i < pRatioPerExposure.Count; i++) {
                var lineSeries = createLineSeries(OxyColors.Green);
                lineSeries.Color = OxyColors.Green;
                lineSeries.Points.Add(new DataPoint(pLeftBoundExposure[i], pRatioPerExposure[i]));
                lineSeries.Points.Add(new DataPoint(pRightBoundExposure[i], pRatioPerExposure[i]));
                plotModel.Series.Add(lineSeries);
            }
            return plotModel;
        }
    }
}

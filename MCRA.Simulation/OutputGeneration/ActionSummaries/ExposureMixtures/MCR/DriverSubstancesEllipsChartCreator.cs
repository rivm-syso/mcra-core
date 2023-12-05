using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DriverSubstancesEllipsChartCreator : DriverCompoundsChartCreatorBase {

        private MaximumCumulativeRatioSection _section;
        private double? _percentage;
        private string _title;
        private string _definition;

        public DriverSubstancesEllipsChartCreator(MaximumCumulativeRatioSection section, double? percentage = null) {
            Height = 400;
            Width = 500;
            _percentage = percentage;
            _section = section;
            _title = _percentage == null ? "(total)" : $"(upper tail {_percentage}%)";
            _definition = _section.IsRiskMcrPlot ? "risk" : "exposure";
        }

        public override string ChartId {
            get {
                var pictureId = "1fd1a2c5-50df-4e5a-b3e8-4d59d1ff6b42";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _percentage);
            }
        }
        public override string Title => $"Using MCR to identify substances that drive cumulative {_definition}, bivariate distributions {_title}.";

        public override PlotModel Create() {
            var xTitle = _section.IsRiskMcrPlot ? "Cumulative risk" : $"Cumulative exposure ({_section.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)})";
            return create(
                _section.DriverSubstanceTargetStatisticsRecords,
                _section.DriverSubstanceTargets,
                _section.RatioCutOff,
                _section.Percentiles,
                _section.CumulativeExposureCutOffPercentage,
                _section.MinimumPercentage,
                xTitle
            );
        }

        private PlotModel create(
            List<DriverSubstanceStatisticsRecord> statistics,
            List<DriverSubstanceRecord> drivers,
            double ratioCutOff,
            double[] percentiles,
            double totalExposureCutOff,
            double minimumPercentage,
            string xTitle
        ) {

            var (plotModel, selectedDrivers, percentilesExposure) = createMCRChart(drivers,
                 ratioCutOff,
                 percentiles,
                 totalExposureCutOff,
                 minimumPercentage,
                 _percentage,
                 xTitle
            );
            var edChiSq = 2d;
            var maxN = statistics.Max(c => c.Number);
            var basePalette = OxyPalettes.Rainbow(selectedDrivers.Count == 1 ? 2 : selectedDrivers.Count);
            var palette = basePalette.Colors.Select(c => OxyColor.FromAColor(100, c));
            var counter = 0;
            statistics = statistics
                .Where(c => selectedDrivers.Select(c => c.Substance).Contains(c.SubstanceCode)
                    && selectedDrivers.Select(c => c.Target).Contains(c.Target))
                .Select(c => c)
                .OrderByDescending(c => c.Number)
                .ThenByDescending(c => c.SubstanceName)
                .ToList();
            foreach (var bivariate in statistics) {
                var xamp = Math.Sqrt(edChiSq) * bivariate.CVCumulativeExposure;
                var yamp = Math.Sqrt(edChiSq) * bivariate.CVRatio;
                var logTotalExposureMedian = Math.Log(bivariate.CumulativeExposureMedian);
                var logRatioMedian = Math.Log(bivariate.RatioMedian);
                var acosR = Math.Acos(bivariate.R);
                var x = new List<double>();
                var y = new List<double>();
                for (int i = 0; i < 362; i++) {
                    var t = i * (2 * Math.PI) / 360;
                    var _x = logTotalExposureMedian + xamp * Math.Cos(t);
                    var _y = logRatioMedian + yamp * Math.Cos(t + acosR);
                    x.Add(Math.Exp(_x));
                    y.Add(Math.Exp(_y));
                }

                var areaSeries = new AreaSeries() {
                    Title = $"{bivariate.SubstanceName}-{bivariate.Target}",
                    Color = basePalette.Colors.ElementAt(counter),
                    Fill = palette.ElementAt(counter),
                    MarkerType = MarkerType.None,
                    StrokeThickness = 1,
                    RenderInLegend = false,
                };
                if (!double.IsNaN(bivariate.R)) {
                    for (int i = 0; i < x.Count; i++) {
                        areaSeries.Points.Add(new DataPoint(x[i], y[i]));
                    }
                } else {
                    areaSeries.Points.Add(new DataPoint(bivariate.CumulativeExposureMedian, bivariate.RatioMedian));
                }
                plotModel.Series.Add(areaSeries);
                counter++;
            }
            return plotModel;
        }
    }
}

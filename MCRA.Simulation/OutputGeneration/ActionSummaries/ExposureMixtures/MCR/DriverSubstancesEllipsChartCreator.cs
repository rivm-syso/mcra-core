using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DriverSubstancesEllipsChartCreator : DriverCompoundsChartCreatorBase {

        private MaximumCumulativeRatioSection _section;
        private double? _percentage;
        public DriverSubstancesEllipsChartCreator(MaximumCumulativeRatioSection section, double? percentage = null) {
            Height = 400;
            Width = 500;
            _percentage = percentage;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "1fd1a2c5-50df-4e5a-b3e8-4d59d1ff6b42";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Using MCR to identify substances that drive cumulative exposures, bivariate distributions.";

        public override PlotModel Create() {
            return create(
                _section.DriverCompoundStatisticsRecords,
                _section.DriverCompounds,
                _section.RatioCutOff,
                _section.Percentiles,
                _section.CumulativeExposureCutOffPercentage,
                _section.MinimumPercentage,
                _section.TargetUnit.GetShortDisplayName(TargetUnit.DisplayOption.AppendBiologicalMatrix)
            );
        }

        private PlotModel create(
            List<DriverCompoundStatisticsRecord> statistics,
            List<DriverCompoundRecord> drivers,
            double ratioCutOff,
            double[] percentiles,
            double totalExposureCutOff,
            double minimumPercentage,
            string intakeUnit
        ) {

            var (plotModel, substanceCodes,  percentilesExposure) = createMCRChart(drivers,
                 ratioCutOff,
                 percentiles,
                 totalExposureCutOff,
                 minimumPercentage,
                 _percentage,
                 intakeUnit
            );
            var edChiSq = 2d;
            var maxN = statistics.Max(c => c.Number);
            var basePalette = OxyPalettes.Rainbow(substanceCodes.Count == 1 ? 2 : substanceCodes.Count);
            var palette = basePalette.Colors.Select(c => OxyColor.FromAColor(100, c));
            var counter = 0;

            foreach (var code in substanceCodes) {
                var bivariate = statistics.First(c => c.CompoundCode == code);
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
                    Title = bivariate.CompoundName,
                    Color = basePalette.Colors.ElementAt(counter),
                    Fill = palette.ElementAt(counter),
                    MarkerType = MarkerType.None,
                    StrokeThickness = 1,
                    RenderInLegend = false,
                };

                for (int i = 0; i < x.Count; i++) {
                    areaSeries.Points.Add(new DataPoint(x[i], y[i]));
                }
                plotModel.Series.Add(areaSeries);
                counter++;
            }
            return plotModel;
        }
    }
}

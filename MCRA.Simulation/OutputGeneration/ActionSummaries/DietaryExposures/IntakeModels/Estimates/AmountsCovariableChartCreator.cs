using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AmountsCovariableChartCreator : ReportLineChartCreatorBase {

        private NormalAmountsModelGraphicsSection _section;
        private List<string> _cofactorLevels;
        private List<ConditionalPrediction> _conditional;
        private List<ConditionalPrediction> _conditionalData;

        public override string ChartId {
            get {
                var pictureId = "3ae5c5cd-160a-4b6f-b042-ca1194a61a27";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Amounts";

        public AmountsCovariableChartCreator(NormalAmountsModelGraphicsSection section) {
            Width = 500;
            Height = 350;
            _section = section;
            _cofactorLevels = section.Predictions.ConditionalPrediction.Select(c => c.Cofactor).Distinct().ToList();
            _conditional = section.Predictions.ConditionalPrediction;
            _conditionalData = section.Predictions.ConditionalData;
        }

        public override PlotModel Create() {
            return create(_section.CovariableName, _section.CofactorName, _cofactorLevels, _conditional, _conditionalData);
        }

        private PlotModel create(string covariableName, string cofactorName, List<string> cofactorLevels, List<ConditionalPrediction> conditional, List<ConditionalPrediction> conditionalData) {
            var plotModel = createDefaultPlotModel();
            var legend = new Legend {
                LegendBackground = OxyColor.FromArgb(200, 255, 255, 255),
                LegendBorder = OxyColors.Undefined,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomLeft,
                LegendFontSize = 13
            };
            plotModel.Legends.Add(legend);
            plotModel.IsLegendVisible = true;

            var lineSeries = new LineSeries();
            plotModel.Series.Add(lineSeries);

            var basePalette = OxyPalettes.BlackWhiteRed(cofactorLevels.Count == 1 ? 2 : cofactorLevels.Count);
            var counter = 0;
            foreach (var level in cofactorLevels) {
                var series = new LineSeries() {
                    Color = basePalette.Colors.ElementAt(counter),
                };
                var qry = _conditional.Where(f => f.Cofactor == level).ToList(); ;
                for (int i = 0; i < qry.Count; i++) {
                    series.Points.Add(new DataPoint(qry[i].Covariable, qry[i].Prediction));
                }
                plotModel.Series.Add(series);

                var scatter = new ScatterSeries() {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 2,
                    MarkerFill = basePalette.Colors.ElementAt(counter),
                    Title = level,
                };

                var yData = new List<double>();
                var xData = new List<double>();
                //data contain labels for cofactor and covariable in contrast to predictions therefor see code below
                var qry1 = _conditionalData;
                if (cofactorLevels.Count > 1) {
                    yData = qry1.Where(f => f.Cofactor == level).Select(c => c.Prediction).ToList();
                    xData = qry1.Where(f => f.Cofactor == level).Select(c => c.Covariable).ToList();
                } else {
                    yData = qry1.Select(c => c.Prediction).ToList();
                    xData = qry1.Select(c => c.Covariable).ToList();
                }
                for (int i = 0; i < yData.Count; i++) {
                    scatter.Points.Add(new ScatterPoint(xData[i], yData[i]));
                }
                plotModel.Series.Add(scatter);
                counter++;
            }

            var horizontalAxis = createLinearAxis(covariableName);
            horizontalAxis.Position = AxisPosition.Bottom;
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = createLinearAxis("transformed amounts");
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }
    }
}






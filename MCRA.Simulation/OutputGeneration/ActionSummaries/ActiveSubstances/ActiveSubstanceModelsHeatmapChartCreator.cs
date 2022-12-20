using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstanceModelsHeatmapChartCreator : OxyPlotChartCreator {

        private const int _cellSize = 24;
        private const int _cutoffSubstancesDetails = 30;
        private const int _cutoffModelsDetails = 10;

        private ActiveSubstancesSummarySection _section;
        private List<string> _substanceNames;
        private List<string> _substanceCodes;

        public ActiveSubstanceModelsHeatmapChartCreator(ActiveSubstancesSummarySection section) {
            _section = section;
            _substanceCodes = section.SubstanceCodes;
            _substanceNames = section.SubstanceNames;
            Height = 250 + Math.Min(_cutoffSubstancesDetails, section.SubstanceCodes.Count) * _cellSize;
            Width = 200 + _section.Records.Count * _cellSize;
        }

        public override string ChartId {
            get {
                var pictureId = "6600CE68-0A2F-4B47-A154-6C34540C092A";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return createHeatmap(_section.Records, _substanceNames, _substanceCodes, _cutoffSubstancesDetails);
        }

        private static PlotModel createHeatmap(List<ActiveSubstanceModelRecord> models, List<string> substanceNames, List<string> substanceCodes, int maxRecords) {
            var plotModel = new PlotModel() {
                Title = "Assessment group memberships",
                TitleFontWeight = FontWeights.Normal,
                TitleFontSize = 12,
                IsLegendVisible = false,
            };

            var matrix = new double[models.Count, substanceCodes.Count]; 
            for (int i = 0; i < models.Count; i++) {
                var model = models[i];
                var membershipsLookup = model.MembershipProbabilities.ToDictionary(r => r.SubstanceCode);
                for (int j = 0; j < substanceCodes.Count; j++) {
                    var substance = substanceCodes[j];
                    membershipsLookup.TryGetValue(substance, out var score);
                    matrix[i,j] = score?.Probability ?? double.NaN;
                }
            }

            var linearColorAxis1 = new LinearColorAxis();
            linearColorAxis1.Position = AxisPosition.Right;
            plotModel.Axes.Add(linearColorAxis1);
            linearColorAxis1.Palette = OxyPalette.Interpolate(255, new[] { OxyColors.Red, OxyColors.Black, OxyColors.Green });

            var linearAxis1 = new LinearAxis() {
                Title = "Substances",
                IsAxisVisible = false,
                MaximumPadding = 0,
                MinimumPadding = 0
            };
            plotModel.Axes.Add(linearAxis1);

            if (substanceCodes.Count < _cutoffSubstancesDetails) {
                var categoryAxis1 = new CategoryAxis() {
                    MinorStep = 1,
                    Position = AxisPosition.Left,
                    FontSize = 9,
                };
                foreach (var name in substanceNames) {
                    var shortname = name.LimitTo(20);
                    categoryAxis1.Labels.Add(shortname);
                }
                plotModel.Axes.Add(categoryAxis1);
            }

            var linearAxis2 = new LinearAxis() {
                Title = "Model",
                IsAxisVisible = false,
                TickStyle = TickStyle.None,
                Position = AxisPosition.Bottom,
                TitlePosition = 1,
                MaximumPadding = 0,
                MinimumPadding = 0,
            };
            plotModel.Axes.Add(linearAxis2);

            var categoryAxis2 = new CategoryAxis() {
                MinorStep = 1,
                Position = AxisPosition.Top,
                Angle = 90,
                FontSize = 9,
            };
            foreach (var model in models) {
                var shortname = model.Code.LimitTo(20);
                categoryAxis2.Labels.Add(shortname);
            }
            plotModel.Axes.Add(categoryAxis2);

            var renderCellLabels = substanceCodes.Count < _cutoffSubstancesDetails
                && models.Count < _cutoffModelsDetails;
            var heatMapSeries = new HeatMapSeries() {
                Interpolate = false,
                X0 = 0,
                X1 = substanceNames.Count,
                Y0 = 0,
                Y1 = models.Count,
                LabelFontSize = renderCellLabels ? .3 : 0,
                Data = matrix,
                RenderMethod = HeatMapRenderMethod.Rectangles,
            };

            plotModel.Series.Add(heatMapSeries);
            return plotModel;
        }
    }
}

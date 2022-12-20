using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstancesChartCreator : OxyPlotChartCreator {

        private const int _cellSize = 15;
        private const int _switchHorizontalVertical = 30;

        private ActiveSubstancesSummarySection _section;
        private List<string> _substanceNames;
        private List<string> _substanceCodes;

        public ActiveSubstancesChartCreator(ActiveSubstancesSummarySection section) {
            _section = section;
            var substances = section.Records.SelectMany(r => r.MembershipProbabilities)
                .GroupBy(r => r.SubstanceCode)
                .OrderBy(r => r.Max(p => p.Probability))
                .Select(r => (
                    Code: r.Key,
                    Name: r.First().SubstanceName
                ))
                .ToList();
            Height = 150 + Math.Min(_switchHorizontalVertical, substances.Count) * _cellSize;
            Width = 800;
            _substanceCodes = substances.Select(r => r.Code).ToList();
            _substanceNames = substances.Select(r => r.Name).ToList();
        }

        public override string ChartId {
            get {
                var pictureId = "8093061E-1016-4DF7-A2CC-41FEAA0914C6";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.Records, _substanceNames, _substanceCodes, _switchHorizontalVertical);
        }

        private static PlotModel create(List<ActiveSubstanceModelRecord> models, List<string> substanceNames, List<string> substanceCodes, int maxRecords) {
            var title = "Assessment group memberships";

            var isCutoff = substanceCodes.Count > maxRecords;
            var split = (int)(maxRecords / 2);
            var skipped = substanceCodes.Count - 2 * split;
            if (isCutoff) {
                title = $"{title} - {split} most and {split} least binding substances";
                var newSubstanceCodes = substanceCodes.Take(split).ToList();
                newSubstanceCodes.Add(null);
                newSubstanceCodes.AddRange(substanceCodes.Skip(substanceCodes.Count - split));

                var newSubstanceNames = substanceNames.Take(split).ToList();
                newSubstanceNames.Add($"[{skipped} substances skipped]");
                newSubstanceNames.AddRange(substanceNames.Skip(substanceNames.Count - split));

                substanceCodes = newSubstanceCodes;
                substanceNames = newSubstanceNames;
            }

            var plotModel = new PlotModel() {
                Title = title,
                LegendPlacement = LegendPlacement.Outside,
                PlotMargins = new OxyThickness(150, double.NaN, double.NaN, double.NaN)
            };

            var horizontalAxis = new LinearAxis() {
                Title = "Membership score",
                Minimum = -0.02,
                Maximum = 1.02,
                MajorStep = 0.2,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Bottom,
            };
            plotModel.Axes.Add(horizontalAxis);

            var categoryAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                IsTickCentered = true,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Left,
                Minimum = -0.5,
                Maximum = substanceNames.Count - .5,
            };
            foreach (var item in substanceNames) {
                categoryAxis.Labels.Add(item);
            }
            plotModel.Axes.Add(categoryAxis);

            if (isCutoff) {
                var lineAnnotation = new LineAnnotation() {
                    Type = LineAnnotationType.Horizontal,
                    Color = OxyColors.Black,
                    Y = split,
                    StrokeThickness = 1,
                    LineStyle = LineStyle.Dash
                };
                plotModel.Annotations.Add(lineAnnotation);
            }

            var palette = CustomPalettes.BlueTone(models.Count);
            var ix = 0;
            foreach (var model in models) {
                var scatterSeries = new ScatterSeries() {
                    Title = model.Name.LimitTo(15),
                    MarkerSize = 5,
                    MarkerType = MarkerType.Circle,
                    MarkerFill = palette.Colors[ix],
                };
                plotModel.Series.Add(scatterSeries);

                var points = new List<ScatterPoint>();
                var bindingEnergiesLookup = model.MembershipProbabilities.ToDictionary(r => r.SubstanceCode);
                for (int j = 0; j < substanceNames.Count; j++) {
                    if (!string.IsNullOrEmpty(substanceCodes[j]) && bindingEnergiesLookup.ContainsKey(substanceCodes[j])) {
                        var x = bindingEnergiesLookup[substanceCodes[j]].Probability;
                        var bpItem = new ScatterPoint(x, j);
                        points.Add(bpItem);
                    }
                }
                scatterSeries.ItemsSource = points;

                ix++;
            }

            return plotModel;
        }

    }
}

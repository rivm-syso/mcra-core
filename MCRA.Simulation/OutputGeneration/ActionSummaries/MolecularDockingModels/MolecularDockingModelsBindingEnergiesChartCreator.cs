using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelsBindingEnergiesChartCreator : ReportChartCreatorBase {

        private const int _cellSize = 15;
        private const int _switchHorizontalVertical = 30;

        private MolecularDockingModelsBindingEnergiesSection _section;
        private List<string> _substanceNames;
        private List<string> _substanceCodes;

        public override string ChartId {
            get {
                var pictureId = "167F825A-0AE8-4C0E-B694-6C98F56C0433";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public MolecularDockingModelsBindingEnergiesChartCreator(MolecularDockingModelsBindingEnergiesSection section) {
            _section = section;
            var substances = section.Records.SelectMany(r => r.BindingEnergies)
                .GroupBy(r => r.SubstanceCode)
                .OrderBy(r => r.Max(be => be.BindingEnergy))
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

        public override PlotModel Create() {
            return createVertical(_section.Records, _substanceCodes, _substanceNames, _switchHorizontalVertical);
        }

        private static PlotModel createVertical(List<MolecularDockingModelBindingEnergiesRecord> models, List<string> substanceCodes, List<string> substanceNames, int maxRecords, bool rescale = true) {
            var title = rescale ? "Docking scores (diff. threshold)" : "Binding energies";

            var isCutoff = substanceCodes.Count > maxRecords;
            var split = maxRecords / 2;
            if (isCutoff) {
                title = $"{title} - {split} least and {split} most binding substances";
                var newSubstanceCodes = substanceCodes.Take(split).ToList();
                newSubstanceCodes.Add(null);
                newSubstanceCodes.AddRange(substanceCodes.Skip(substanceCodes.Count - split));

                var newSubstanceNames = substanceNames.Take(split).ToList();
                newSubstanceNames.Add("...");
                newSubstanceNames.AddRange(substanceNames.Skip(substanceNames.Count - split));

                substanceCodes = newSubstanceCodes;
                substanceNames = newSubstanceNames;
            }

            var plotModel = new PlotModel() {
                Title = title,
                TitleFontWeight = FontWeights.Normal,
                TitleFontSize = 13,
                IsLegendVisible = true,
                ClipTitle = false
            };

            var Legend = new Legend {
                LegendPlacement = LegendPlacement.Outside,
            };
            plotModel.Legends.Add(Legend);

            var horizontalAxis = new LinearAxis() {
                Title = rescale ? "Difference with threshold" : "Binding energy",
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineStyle = LineStyle.None,
                MinorTickSize = 0,
                Position = AxisPosition.Bottom,
            };
            plotModel.Axes.Add(horizontalAxis);

            var verticalAxis = new CategoryAxis() {
                MinorStep = 1,
                GapWidth = 0.1,
                IsTickCentered = true,
                TextColor = OxyColors.Black,
                Position = AxisPosition.Left,
                Minimum = -.5,
                Maximum = substanceNames.Count - .5,
            };
            plotModel.Axes.Add(verticalAxis);
            foreach (var substance in substanceNames) {
                verticalAxis.Labels.Add(substance);
            }

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

            var palette = CustomPalettes.DistinctTone(models.Count);
            var ix = 0;
            foreach (var model in models) {
                var scatterSeries = new ScatterSeries() {
                    Title = model.Name,
                    MarkerSize = 5,
                    MarkerType = MarkerType.Circle,
                    MarkerFill = palette.Colors[ix],
                };
                plotModel.Series.Add(scatterSeries);

                if (!rescale) {
                    var lineAnnotation = new LineAnnotation() {
                        Type = LineAnnotationType.Vertical,
                        Color = palette.Colors[ix],
                        X = model.Threshold,
                        StrokeThickness = 3,
                        LineStyle = LineStyle.Solid
                    };
                    plotModel.Annotations.Add(lineAnnotation);
                }

                var points = new List<ScatterPoint>();
                var bindingEnergiesLookup = model.BindingEnergies.ToDictionary(r => r.SubstanceCode);
                for (int j = 0; j < substanceNames.Count; j++) {
                    if (!string.IsNullOrEmpty(substanceCodes[j]) && bindingEnergiesLookup.ContainsKey(substanceCodes[j])) {
                        var x = rescale ? bindingEnergiesLookup[substanceCodes[j]].BindingEnergy - model.Threshold : bindingEnergiesLookup[substanceCodes[j]].BindingEnergy;
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

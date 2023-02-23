using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmDataBoxPlotChartCreator : HbmDataBoxPlotChartCreatorBase {

        private HbmSamplesBySamplingMethodSubstanceSection _section;

        public HbmDataBoxPlotChartCreator(
            HbmSamplesBySamplingMethodSubstanceSection section,
            string concentrationUnit
        ) {
            _section = section;
            _concentrationUnit = concentrationUnit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmPercentilesRecords.Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override string ChartId {
            get {
                var pictureId = "df9b4f47-bd35-4b92-86b9-91b1a53bf866";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var matrices = _section.Records.Select(r => r.BiologicalMatrix).Distinct();
                var description = $"Boxplots of positive/quantified HBM substance concentration measurements";
                description += $" in {string.Join(", ", matrices)}";
                if (_section.Records.Count == 1) {
                    description += $" (n={_section.Records.First().PositiveMeasurements})";
                }
                description += ".";
                description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95 and LOR (red bar).";
                return description;
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmPercentilesRecords, $"Concentration ({_concentrationUnit})");
        }
    }
}



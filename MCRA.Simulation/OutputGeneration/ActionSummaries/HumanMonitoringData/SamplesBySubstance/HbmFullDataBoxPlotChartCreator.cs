using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmFullDataBoxPlotChartCreator : HbmDataBoxPlotChartCreatorBase {

        private readonly HbmSamplesBySamplingMethodSubstanceSection _section;
        private readonly BiologicalMatrix _biologicalMatrix;

        public HbmFullDataBoxPlotChartCreator(
            HbmSamplesBySamplingMethodSubstanceSection section,
            BiologicalMatrix biologicalMatrix
        ) {
            _section = section;
            _biologicalMatrix = biologicalMatrix;
            _concentrationUnit = _section.HbmPercentilesRecords[biologicalMatrix].FirstOrDefault().Unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmPercentilesAllRecords[biologicalMatrix].Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override string ChartId {
            get {
                var pictureId = "ca73c617-19f8-488b-a75a-8fab0c9e2107" + (int)_biologicalMatrix;
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title {
            get {
                var description = "Boxplots of (all) HBM substance concentration measurements";
                if (_section.Records.Count == 1) {
                    description += $" (n={_section.Records.First().NumberOfSamples - _section.Records.First().MissingValueMeasurements})";
                }
                description += ".";
                description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95, LOR (red bar) and outliers outside range (Q1 - 3 * IQR , Q3 + 3 * IQR).";
                return description;
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmPercentilesAllRecords[_biologicalMatrix], $"Concentration ({_concentrationUnit})");
        }
    }
}

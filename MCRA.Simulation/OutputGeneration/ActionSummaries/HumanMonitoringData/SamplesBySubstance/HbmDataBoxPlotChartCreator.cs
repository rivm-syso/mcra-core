using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmDataBoxPlotChartCreator : HbmDataBoxPlotChartCreatorBase {

        private HbmSamplesBySamplingMethodSubstanceSection _section;
        private readonly BiologicalMatrix _biologicalMatrix;

        public HbmDataBoxPlotChartCreator(
            HbmSamplesBySamplingMethodSubstanceSection section,
            BiologicalMatrix biologicalMatrix
        ) {
            _section = section;
            _biologicalMatrix = biologicalMatrix;
            _concentrationUnit = _section.HbmPercentilesRecords[biologicalMatrix].FirstOrDefault()?.Unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmPercentilesRecords[biologicalMatrix].Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override string ChartId {
            get {
                var pictureId = "df9b4f47-bd35-4b92-86b9-91b1a53bf866";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + (int)_biologicalMatrix);
            }
        }

        public override string Title {
            get {
                var description = $"Boxplots of positive/quantified HBM substance concentration measurements";
                if (_section.Records.Count == 1) {
                    description += $" (n={_section.Records.First().PositiveMeasurements})";
                }
                description += ".";
                description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95, LOR (red bar) and outliers outside range (Q1 - 3 * IQR , Q3 + 3 * IQR).";
                return description;
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmPercentilesRecords[_biologicalMatrix], $"Concentration ({_concentrationUnit})");
        }
    }
}



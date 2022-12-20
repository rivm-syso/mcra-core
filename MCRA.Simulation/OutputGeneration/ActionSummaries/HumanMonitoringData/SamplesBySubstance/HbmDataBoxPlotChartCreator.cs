using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using System;
using System.Linq;

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
                if (_section.Records.Count == 1) {
                    return $"Positive concentrations (n={_section.Records.First().PositiveMeasurements}). Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95 and LOR (red bar).";
                } else {
                    return "Positive concentrations. Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95 and LOR (red bar).";
                }
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmPercentilesRecords, $"Concentration ({_concentrationUnit})");
        }
    }
}



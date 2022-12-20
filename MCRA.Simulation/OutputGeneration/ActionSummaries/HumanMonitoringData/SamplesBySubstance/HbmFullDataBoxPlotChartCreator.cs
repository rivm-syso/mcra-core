using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using System;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmFullDataBoxPlotChartCreator : HbmDataBoxPlotChartCreatorBase {

        private readonly HbmSamplesBySamplingMethodSubstanceSection _section;

        public HbmFullDataBoxPlotChartCreator(
            HbmSamplesBySamplingMethodSubstanceSection section,
            string concentrationUnit
        ) {
            _section = section;
            _concentrationUnit = concentrationUnit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmPercentilesAllRecords.Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override string ChartId {
            get {
                var pictureId = "ca73c617-19f8-488b-a75a-8fab0c9e2107";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }


        public override string Title {
            get {
                if (_section.Records.Count == 1) {
                    return $"All concentrations (n={_section.Records.First().NumberOfSamples - _section.Records.First().MissingValueMeasurements}): lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95 and LOR (red bar).";
                } else {
                    return "All concentrations. Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95 and LOR (red bar).";
                }
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmPercentilesAllRecords, $"Concentration ({_concentrationUnit})");
        }
    }
}

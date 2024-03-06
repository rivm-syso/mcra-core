using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmAllDataBoxPlotChartCreator : HbmDataBoxPlotChartCreatorBase {

        private readonly HbmSamplesBySamplingMethodSubstanceSection _section;
        private readonly HumanMonitoringSamplingMethod _samplingMethod;

        public HbmAllDataBoxPlotChartCreator(
            HbmSamplesBySamplingMethodSubstanceSection section,
            HumanMonitoringSamplingMethod samplingMethod
        ) {
            _section = section;
            _samplingMethod = samplingMethod;
            _concentrationUnit = _section.HbmPercentilesRecords[samplingMethod].FirstOrDefault()?.Unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmPercentilesAllRecords[samplingMethod].Count * _cellSize, 80);
            BoxColor = OxyColors.Orange;
            StrokeColor = OxyColors.DarkOrange;
        }

        public override string ChartId {
            get {
                var pictureId = "ca73c617-19f8-488b-a75a-8fab0c9e2107";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _samplingMethod.Code);
            }
        }

        public override string Title {
            get {
                var description = $"Boxplots of (all) HBM substance concentration measurements in {_samplingMethod.BiologicalMatrix.GetDisplayName(true)} ({_samplingMethod.SampleTypeCode?.ToLower()})";
                if (_section.Records.Count == 1) {
                    description += $" (n={_section.Records.First().SamplesTotal - _section.Records.First().MissingValueMeasurementsTotal})";
                }
                description += ".";
                description += " Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95, LOR (red bar) and outliers outside range (Q1 - 3 * IQR , Q3 + 3 * IQR).";
                return description;
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmPercentilesAllRecords[_samplingMethod], $"Concentration ({_concentrationUnit})");
        }
    }
}

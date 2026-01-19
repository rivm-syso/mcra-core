using CommandLine;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute {
    public class ExternalExposuresByRouteBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExternalExposuresByRoutePercentilesRecord> _records;
        private readonly ExternalExposuresByRouteSection _section;
        private readonly ExposureUnitTriple _exposureUnit;

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public ExternalExposuresByRouteBoxPlotChartCreator(
            ExternalExposuresByRouteSection section,
            List<ExternalExposuresByRoutePercentilesRecord> records,
            ExposureUnitTriple exposureUnit
        ) {
            _section = section;
            _records = records;
            _exposureUnit = exposureUnit;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                return StringExtensions.CreateFingerprint(_section.SectionId + _section.PictureId
                    + "75b38c01-170d-46a4-ad69-6627e589f77c");
            }
        }

        public override PlotModel Create() {
            return create(
                [.. _records.Cast<BoxPlotChartRecord>()],
                $"Exposure ({_exposureUnit.GetShortDisplayName()})",
                false,
                true
            );
        }
    }
}

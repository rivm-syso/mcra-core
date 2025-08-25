using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodChartCreator : AttributableBodChartCreatorBase {

        public AttributableBodChartCreator(
            List<AttributableBodSummaryRecord> records,
            string sectionId
        ) : base(records, sectionId) {
        }

        public override string Title => "Attributable burden &amp; cumulative percentage.";

        public override string ChartId {
            get {
                var pictureId = "764e10fb-d6e0-4690-b5ce-ffdbd3643d5d";
                return StringExtensions.MungeToGuid(_sectionId, pictureId + _population + _bodIndicator + _sourceIndicators + _erfCode);
            }
        }

        public override PlotModel Create() => create(
            bars: [.. _records
                .Select(c => (
                    Value: c.AttributableBod,
                    Lower: c.LowerAttributableBod,
                    Upper: c.UpperAttributableBod
                ))],
            cumulative: [.. _records
                .Select(c => (
                    Value: c.CumulativeAttributableBod,
                    Lower: c.LowerCumulativeAttributableBod,
                    Upper: c.UpperCumulativeAttributableBod
                ))],
            labels: [.. _records.Select(c => c.ExposureBin)],
            unit: _unit,
            leftYAxisTitle: $"Attributable Burden ({_bodIndicator})",
            uncertainty: _records.SelectMany(c => c.AttributableBods).Any()
        );
    }
}

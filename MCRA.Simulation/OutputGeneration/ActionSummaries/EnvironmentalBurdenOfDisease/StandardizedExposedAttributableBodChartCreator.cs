using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class StandardizedExposedAttributableBodChartCreator : AttributableBodChartCreatorBase {

        public StandardizedExposedAttributableBodChartCreator(
            List<AttributableBodSummaryRecord> records,
            string sectionId
        ) : base(records, sectionId) {
        }

        public override string Title => "Standardized exposed attributable burden &amp; cumulative percentage.";

        public override string ChartId {
            get {
                var pictureId = "32eee09f-61c2-4408-aace-91baa9ead5b8";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _population + _bodIndicator + _erfCode);
            }
        }

        public override PlotModel Create() => create(
            bars: _records
                .Select(c => (
                    Value: c.StandardizedExposedAttributableBod,
                    Lower: c.LowerStandardizedExposedAttributableBod,
                    Upper: c.UpperStandardizedExposedAttributableBod
                ))
                .ToList(),
            cumulative: _records
                .Select(c => (
                    Value: c.CumulativeStandardizedExposedAttributableBod,
                    Lower: c.LowerCumulativeStandardizedExposedAttributableBod,
                    Upper: c.UpperCumulativeStandardizedExposedAttributableBod
                ))
                .ToList(),
            labels: _records.Select(c => c.ExposureBin).ToList(),
            unit: _unit,
            leftYAxisTitle: $"Attributable Burden ({_bodIndicator} per 100k exposed)",
            uncertainty: _records.SelectMany(c => c.AttributableBods).Any()
        );
    }
}

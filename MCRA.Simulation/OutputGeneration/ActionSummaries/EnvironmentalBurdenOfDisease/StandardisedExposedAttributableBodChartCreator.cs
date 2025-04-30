using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class StandardisedExposedAttributableBodChartCreator : AttributableBodChartCreatorBase {

        public StandardisedExposedAttributableBodChartCreator(
            List<AttributableBodSummaryRecord> records,
            string sectionId
        ) : base(records, sectionId) {
        }

        public override string Title => "Standardised exposed attributable burden &amp; cumulative percentage.";

        public override string ChartId {
            get {
                var pictureId = "32eee09f-61c2-4408-aace-91baa9ead5b8";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _population + _bodIndicator + _erfCode);
            }
        }

        public override PlotModel Create() => create(
            bars: _records
                .Select(c => (
                    Value: c.StandardisedExposedAttributableBod,
                    Lower: c.LowerStandardisedExposedAttributableBod,
                    Upper: c.UpperStandardisedExposedAttributableBod
                ))
                .ToList(),
            cumulative: _records
                .Select(c => (
                    Value: c.CumulativeStandardisedExposedAttributableBod,
                    Lower: c.LowerCumulativeStandardisedExposedAttributableBod,
                    Upper: c.UpperCumulativeStandardisedExposedAttributableBod
                ))
                .ToList(),
            labels: _records.Select(c => c.ExposureBin).ToList(),
            unit: _unit,
            leftYAxisTitle: $"Attributable Burden ({_bodIndicator} per 100k exposed)",
            uncertainty: _records.SelectMany(c => c.AttributableBods).Any()
        );
    }
}

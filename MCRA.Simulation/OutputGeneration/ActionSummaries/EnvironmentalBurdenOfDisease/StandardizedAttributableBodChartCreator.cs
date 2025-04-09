using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class StandardizedAttributableBodChartCreator : AttributableBodChartCreatorBase {

        public StandardizedAttributableBodChartCreator(
            List<AttributableBodSummaryRecord> records,
            string sectionId
        ) : base(records, sectionId) {
        }

        public override string Title => "Standardized attributable burden &amp; cumulative percentage.";

        public override string ChartId {
            get {
                var pictureId = "bd49c628-fc08-482d-8878-cf3fcecbdc1d";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _population + _bodIndicator + _erfCode);
            }
        }

        public override PlotModel Create() => create(
            bars: _records
                .Select(c => (
                    Value: c.StandardizedAttributableBod,
                    Lower: c.LowerStandardizedAttributableBod,
                    Upper: c.UpperStandardizedAttributableBod
                ))
                .ToList(),
            cumulative: _records
                .Select(c => (
                    Value: c.CumulativeAttributableBod,
                    Lower: c.LowerCumulativeAttributableBod,
                    Upper: c.UpperCumulativeAttributableBod
                ))
                .ToList(),
            labels: _records.Select(c => c.ExposureBin).ToList(),
            unit: _unit,
            leftYAxisTitle: $"Attributable Burden ({_bodIndicator} per 100k)",
            uncertainty: _records.SelectMany(c => c.AttributableBods).Any()
        );
    }
}

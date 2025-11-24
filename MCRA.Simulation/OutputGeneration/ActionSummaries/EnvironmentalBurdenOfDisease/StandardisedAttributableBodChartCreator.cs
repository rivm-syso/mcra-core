using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class StandardisedAttributableBodChartCreator : AttributableBodChartCreatorBase {

        public StandardisedAttributableBodChartCreator(
            List<AttributableBodSummaryRecord> records,
            EnvironmentalBodStandardisationMethod standardisationMethod,
            string sectionId
        ) : base(records, standardisationMethod, sectionId) {
        }

        public override string Title => $"EBD per {_standardisationMethod.GetDisplayName()} &amp; cumulative percentage.";

        public override string ChartId {
            get {
                var pictureId = "bd49c628-fc08-482d-8878-cf3fcecbdc1d";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _population + _bodIndicator + _erfCode);
            }
        }

        public override PlotModel Create() => create(
            bars: [.. _records
                .Select(c => (
                    Value: c.StandardisedAttributableBod,
                    Lower: c.LowerStandardisedAttributableBod,
                    Upper: c.UpperStandardisedAttributableBod
                ))],
            cumulative: [.. _records
                .Select(c => (
                    Value: c.CumulativeAttributableBod,
                    Lower: c.LowerCumulativeAttributableBod,
                    Upper: c.UpperCumulativeAttributableBod
                ))],
            labels: [.. _records.Select(c => c.ExposureBin)],
            unit: _unit,
            leftYAxisTitle: $"Attributable Burden ({_bodIndicator} per {_standardisationMethod.GetShortDisplayName()})",
            uncertainty: _records.SelectMany(c => c.AttributableBods).Any()
        );
    }
}

using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductPercentilesDataChartCreator : CumulativeChartCreatorBase {

        private readonly PercentileRecord _percentileRecord;
        private readonly string _intakeUnit;
        private readonly string _sectionId;

        public ConsumerProductPercentilesDataChartCreator(string sectionid, PercentileRecord percentileRecord, string intakeUnit) {
            Width = 500;
            Height = 350;
            _sectionId = sectionid + percentileRecord.ExposureSetCode;
            _percentileRecord = percentileRecord;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "9598f01f-3ac3-4623-805d-bcddb1916ba1";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }
        public override string Title => $"Cumulative consumer product exposure distribution.";

        public override PlotModel Create() {
            return createPlotModel(
                _percentileRecord.Percentiles,
                null,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}

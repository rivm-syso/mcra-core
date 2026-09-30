using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeDistributionsTimePointBoxPlotChartCreator<S, T>
        : HbmConcentrationsBoxPlotChartCreatorBase<S, T>
        where S : IExposureContributorKey, new()
        where T : HbmBoxPlotRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly ExposureType _exposureType;
        private readonly string _sectionId;

        public HbmCumulativeDistributionsTimePointBoxPlotChartCreator(List<T> records, ExposureType exposureType, string sectionId) {
            _records = records;
            _exposureType = exposureType;
            _sectionId = sectionId;
            Width = 500;
            Height = 80 + Math.Max(_cellSize, 80);
        }

        public override string Title {
            get {
                var dayStr = _exposureType == ExposureType.Acute ? "day" : string.Empty;
                return $"Cumulative HBM individual {dayStr} concentrations. " + base.Title;
            }
        }

        public override string ChartId {
            get {
                var pictureId = "98c63404-f9a4-4019-8589-e9db8b83f114";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var unit = _records.First().Unit;
            return create(_records, $"Cumulative concentration ({unit})", false);
        }
    }
}

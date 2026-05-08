using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeDistributionsBoxPlotChartCreator<S, T>
        : HbmConcentrationsBoxPlotChartCreatorBase<S, T>
        where S : IHbmExposureContributorKey, new()
        where T : HbmBoxPlotRecordBase<S>, new() {

        private readonly T _record;
        private readonly ExposureType _exposureType;
        private readonly string _sectionId;

        public HbmCumulativeDistributionsBoxPlotChartCreator(T record, ExposureType exposureType, string sectionId) {
            _record = record;
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
                var pictureId = "b387b501-8483-4e5f-b9ec-61543576b0a5";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var unit = _record.Unit;
            return create([_record], $"Cumulative concentration ({unit})", false);
        }
    }
}

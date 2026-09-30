using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmConcentrationBySubstanceTimepointBoxPlotChartCreator<S, T> : HbmConcentrationsBoxPlotChartCreatorBase<S, T> 
        where S : IExposureContributorKey, new()
        where T : HbmBoxPlotRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly ExposureTarget _target;
        private readonly string _sectionId;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public HbmConcentrationBySubstanceTimepointBoxPlotChartCreator(
            List<T> records,
            ExposureTarget target,
            string sectionId,
            string unit,
            bool showOutliers
        ) {
            _records = records;
            _target = target;
            _sectionId = sectionId;
            _unit = unit;
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "fd8777af-c7e6-46c2-9af4-26af6dfaa1ef";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _target.Code);
            }
        }

        public override PlotModel Create() {
            return create(_records, $"Concentration ({_unit})", _showOutliers);
        }
    }
}

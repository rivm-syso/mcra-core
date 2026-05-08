using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmConcentrationBySubstanceBoxPlotChartCreator<S, T> : HbmConcentrationsBoxPlotChartCreatorBase<S, T> 
        where S : IHbmExposureContributorKey, new()
        where T : HbmBoxPlotRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly ExposureTarget _target;
        private readonly string _sectionId;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public HbmConcentrationBySubstanceBoxPlotChartCreator(
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
                var pictureId = "68f6d4e5-6076-4d3d-9b55-8d5b67d38fd0";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _target.Code);
            }
        }

        public override PlotModel Create() {
            return create(_records, $"Concentration ({_unit})", _showOutliers);
        }
    }
}

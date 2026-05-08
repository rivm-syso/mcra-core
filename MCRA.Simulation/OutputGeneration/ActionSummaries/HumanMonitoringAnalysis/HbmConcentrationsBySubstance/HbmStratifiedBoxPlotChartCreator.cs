using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmStratifiedBoxPlotChartCreator<S, T> : CategorizedBoxPlotChartCreatorBase
        where S : IExposureContributorKey, new()
        where T : HbmBoxPlotRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly string _unit;
        private readonly bool _showOutliers;
        private readonly string _sectionId;

        public HbmStratifiedBoxPlotChartCreator(
            List<T> records,
            ExposureTarget target,
            string sectionId,
            string unit,
            bool showOutliers
        ) {
            _records = records;
            _unit = unit;
            _showOutliers = showOutliers;
            _sectionId = sectionId;
            Width = 600;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "9eb29881-604d-4c71-a606-5d939d4851f9";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var collection = new List<(string stratifier, BoxPlotChartRecord bps)>();
            foreach (var record in _records) {
                var stratifier = record.Stratification;
                collection.Add((stratifier, record));
            }

            return create(
                records: collection,
                labelHorizontalAxis: $"Concentrations ({_unit})",
                showOutliers: _showOutliers
            );
        }
    }
}

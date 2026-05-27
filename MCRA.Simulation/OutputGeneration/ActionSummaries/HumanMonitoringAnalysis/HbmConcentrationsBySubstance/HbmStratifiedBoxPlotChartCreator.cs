using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmStratifiedBoxPlotChartCreator<S, T> : CategorizedBoxPlotChartCreatorBase
        where S : IExposureContributorKey, new()
        where T : HbmBoxPlotRecordBase<S>, new() {

        private readonly string _sectionId;
        private readonly List<T> _records;
        private readonly string _unit;
        private readonly string _descriptorName;
        private readonly List<OutputStratificationVariable> _stratificationVariables;
        private readonly bool _showOutliers;

        public HbmStratifiedBoxPlotChartCreator(
            List<T> records,
            ExposureTarget target,
            string sectionId,
            string unit,
            bool showOutliers,
            List<OutputStratificationVariable> stratificationVariables
        ) {
            _records = records;
            _unit = unit;
            _showOutliers = showOutliers;
            _sectionId = sectionId;
            _stratificationVariables = stratificationVariables;
            Width = 600;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var baseId = "9eb29881-604d-4c71-a606-5d939d4851f9";
                var stratificationId = _stratificationVariables?.Count > 0
                    ? string.Join("_", _stratificationVariables) : string.Empty;
                return StringExtensions.CreateFingerprint(
                    baseId + _sectionId + _descriptorName + stratificationId
                );
            }
        }

        public override PlotModel Create() {
            return create(
                records: [.. _records.Cast<CategorizedBoxPlotChartRecord>()],
                labelHorizontalAxis: $"Concentrations ({_unit})",
                showOutliers: _showOutliers,
                showLabels:true,
                outliersLimit: 100,
                stratificationVariables: _stratificationVariables
            );
        }
    }
}

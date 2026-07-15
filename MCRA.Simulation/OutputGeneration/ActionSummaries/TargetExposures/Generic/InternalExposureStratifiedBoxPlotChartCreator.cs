using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposureStratifiedBoxPlotChartCreator<S, T> : CategorizedBoxPlotChartCreatorBase
        where S : IContributorKey, new()
        where T : InternalExposureBoxPlotRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly string _unit;
        private readonly string _descriptorName;
        private readonly List<OutputStratificationVariable> _stratificationVariables;
        private readonly bool _showOutliers;

        public InternalExposureStratifiedBoxPlotChartCreator(
            string descriptorName,
            List<T> records,
            TargetUnit unit,
            bool showOutliers,
            List<OutputStratificationVariable> stratificationVariables
        ) {
            _records = records;
            _unit = unit.GetShortDisplayName();
            _showOutliers = showOutliers;
            _descriptorName = descriptorName;
            _stratificationVariables = stratificationVariables;
            Width = 600;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string Title => $"Exposures by {_descriptorName} boxplots. Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override string ChartId {
            get {
                var baseId = "e317ef0e-beee-4cff-b635-00fa4114c283";
                var stratificationId = _stratificationVariables?.Count > 0
                    ? string.Join("_", _stratificationVariables) : string.Empty;
                return StringExtensions.CreateFingerprint(
                    baseId + _descriptorName + stratificationId
                );
            }
        }

        public override PlotModel Create() {
            return create(
                records: [.. _records
                    .OrderBy(c => c.GetLabel())
                    .ThenBy(c => c.Stratification)
                    .Cast<CategorizedBoxPlotChartRecord>()
                ],
                labelHorizontalAxis: $"Exposure ({_unit})",
                showOutliers: _showOutliers,
                stratificationVariables: _stratificationVariables
            );
        }
    }
}

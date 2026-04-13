using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposureStratifiedBoxPlotChartCreator<S, T> : CategorizedBoxPlotChartCreatorBase
        where S : IExposureContributorKey, new()
        where T : InternalExposureBoxPlotRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly string _unit;
        private readonly string _descriptorName;
        private readonly bool _showOutliers;
        private readonly bool _stratified;

        public InternalExposureStratifiedBoxPlotChartCreator(
            string sectionName,
            List<T> records,
            TargetUnit unit,
            bool showOutliers,
            bool stratified
        ) {
            _records = records;
            _unit = unit.GetShortDisplayName();
            _stratified = stratified;
            _showOutliers = showOutliers;
            _descriptorName = sectionName;
            Width = 600;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string Title => $"Exposures by {_descriptorName} boxplots. Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override string ChartId {
            get {
                var pictureId = "e317ef0e-beee-4cff-b635-00fa4114c283";
                return StringExtensions.CreateFingerprint(
                    _descriptorName + pictureId + _stratified.ToString()
                );
            }
        }

        public override PlotModel Create() {
            var results = _records
                .OrderBy(c => c.GetLabel())
                .Select(c =>  (
                    stratifier: c.Stratification,
                    descriptor: getLabel(c.GetLabel(), c.Stratification),
                    record: c as BoxPlotChartRecord
                ))
                .ToList();
            return create(
                records: results,
                labelHorizontalAxis: $"Exposure ({_unit})",
                showOutliers: _showOutliers
            );
        }
    }
}

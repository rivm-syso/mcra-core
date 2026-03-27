using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposureBoxPlotChartCreator<S, T> : BoxPlotChartCreatorBase 
        where S : IExposureContributorKey, new()
        where T : InternalExposureBoxPlotRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly string _unit;
        private readonly string _descriptorName;
        private readonly bool _showOutliers;
        private readonly bool _stratified;

        public InternalExposureBoxPlotChartCreator(
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
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string Title => $"Exposures by {_descriptorName} boxplots. Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override string ChartId {
            get {
                var pictureId = "1c7a3cb3-c55b-41f9-9ba9-151571d3ab84";
                return StringExtensions.CreateFingerprint(
                    _descriptorName + pictureId + _stratified.ToString()
                );
            }
        }

        public override PlotModel Create() {
            return create(
                records: [.. _records.Cast<BoxPlotChartRecord>()],
                labelHorizontalAxis: $"Exposure ({_unit})",
                showOutliers: _showOutliers
            );
        }
    }
}

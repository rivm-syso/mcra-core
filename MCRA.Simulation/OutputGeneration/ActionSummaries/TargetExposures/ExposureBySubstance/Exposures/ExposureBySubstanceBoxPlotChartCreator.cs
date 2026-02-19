using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureBySubstanceBoxPlotChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExposureBySubstancePercentileRecord> _records;
        private readonly string _unit;
        private readonly bool _showOutliers;
        private readonly bool _stratified;
        private readonly ExposureBySubstanceSection _section;

        public ExposureBySubstanceBoxPlotChartCreator(
            ExposureBySubstanceSection section,
            List<ExposureBySubstancePercentileRecord> records,
            TargetUnit unit,
            bool showOutliers,
            bool stratified
        ) {
            _section = section;
            _records = records;
            _unit = unit.GetShortDisplayName();
            _showOutliers = showOutliers;
            _stratified = stratified;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string Title {
            get {
                var title = "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";
                return title;
            }
        }

        public override string ChartId {
            get {
                var pictureId = "21c0166a-9342-4694-890a-e0f59ef91865";
                return StringExtensions.CreateFingerprint(
                    _section.SectionId + pictureId + _stratified.ToString()
                );
            }
        }

        public override PlotModel Create() {
            return create(
                records: _records.Cast<BoxPlotChartRecord>().ToList(),
                labelHorizontalAxis: $"Exposure ({_unit})",
                showOutliers: _showOutliers
            );
        }
    }
}

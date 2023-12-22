using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PopulationPieChartCreator : ReportPieChartCreatorBase {

        private List<SubGroupComponentSummaryRecord> _records;
        private string _sectionId;
        private int _clusterId;

        public PopulationPieChartCreator(string sectionId, List<SubGroupComponentSummaryRecord> records, int clusterId) {
            Width = 500;
            Height = 350;
            _records = records;
            _clusterId = clusterId;
            _sectionId = sectionId;
        }

        public override string ChartId {
            get {
                var pictureId = "a7cdb7b3-c1a7-4ea8-9e63-b3feb73a7a82";
                return StringExtensions.CreateFingerprint(_sectionId + _clusterId + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _records.OrderByDescending(r => r.PercentageAll).ToList();
            var pieSlices = records
                .Where(r => r.PercentageAll > 0)
                .Select(c => new PieSlice($"component {c.ComponentNumber}", c.PercentageAll))
                .ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.Monochrome(noSlices, 0.7, .2, .2, 1, 1);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

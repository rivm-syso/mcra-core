using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ClusterPieChartCreator : PieChartCreatorBase {

        private List<SubGroupComponentSummaryRecord> _records;
        private string _sectionId;
        private int _clusterId;

        public ClusterPieChartCreator(string sectionId, List<SubGroupComponentSummaryRecord> records, int clusterId) {
            Width = 500;
            Height = 350;
            _records = records;
            _clusterId = clusterId;
            _sectionId = sectionId;
        }

        public override string ChartId {
            get {
                var pictureId = "d1aecde9-d61d-4e11-8b3f-27414666c472";
                return StringExtensions.CreateFingerprint(_sectionId + _clusterId + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _records.OrderByDescending(r => r.Percentage).ToList();
            var pieSlices = records
                .Where(r => r.Percentage > 0)
                .Select(c => new PieSlice($"component {c.ComponentNumber}", c.Percentage))
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

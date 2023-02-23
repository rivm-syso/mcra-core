using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NMFPieChartCreator : PieChartCreatorBase {

        private List<SubstanceComponentRecord> _records;
        private int _componentNumber;

        public NMFPieChartCreator(List<SubstanceComponentRecord> records, int componentNumber) {
            Width = 500;
            Height = 350;
            _records = records;
            _componentNumber = componentNumber;
        }

        public override string ChartId {
            get {
                var pictureId = "bfa67ea5-61c1-4269-95c6-c3914178bdb2";
                return StringExtensions.CreateFingerprint(_componentNumber + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _records.OrderByDescending(r => r.NmfValue).ToList();
            var pieSlices = records
                .Where(r => r.NmfValue > 0)
                .Select(c => new PieSlice(c.SubstanceName, c.NmfValue))
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
            var palette = CustomPalettes.Monochrome(noSlices, 0.5883, .2, .2, 1, 1);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

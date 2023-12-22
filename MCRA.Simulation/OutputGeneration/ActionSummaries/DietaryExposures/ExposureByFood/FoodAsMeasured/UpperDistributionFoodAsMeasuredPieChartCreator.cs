using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionFoodAsMeasuredPieChartCreator : ReportPieChartCreatorBase {

        private UpperDistributionFoodAsMeasuredSection _section;
        private List<DistributionFoodRecord> _records;
        private bool _isUncertainty;
        private int _counter;

        public UpperDistributionFoodAsMeasuredPieChartCreator(
            UpperDistributionFoodAsMeasuredSection section, 
            List<DistributionFoodRecord> records, 
            bool isUncertainty, 
            int counter = 0
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _records = records;
            _isUncertainty = isUncertainty;
            _counter = counter;
        }

        public override string ChartId {
            get {
                var pictureId = "9810ece0-8490-4c1e-ab72-9a8391873fc6";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _counter);
            }
        }
        public override string Title => "Contribution to upper exposure distribution for modelled foods.";

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _records.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice(c.FoodName, c.MeanContribution)).ToList();
                return create(pieSlices);
            } else {
                var records = _records.OrderByDescending(r => r.Contribution).ToList();
                var pieSlices = records
                    .Where(r => r.Contribution > 0)
                    .Select(c => new PieSlice(c.FoodName, c.Contribution)).ToList();
                return create(pieSlices);
            }
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GreenToneReverse(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

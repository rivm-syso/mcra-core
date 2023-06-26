using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionTDSFoodAsMeasuredPieChartCreator : PieChartCreatorBase {

        private TotalDistributionTDSFoodAsMeasuredSection _section;

        public TotalDistributionTDSFoodAsMeasuredPieChartCreator(TotalDistributionTDSFoodAsMeasuredSection section) {
            Width = 500;
            Height = 350;
            ExplodeFirstSlide = true;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "9471c600-0c63-455c-a801-92be3e15f47b";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Contribution to total exposure distribution for TDS vs Read Across translations.";

        public override PlotModel Create() {
            var records = new List<TDSReadAcrossFoodRecord> {
                _section.Records.First()
            };
            records.AddRange(_section.Records.Skip(1).OrderByDescending(r => r.Contribution));
            var pieSlices = records.Select(c => new PieSlice(c.FoodName, c.Contribution)).ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.BeachTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

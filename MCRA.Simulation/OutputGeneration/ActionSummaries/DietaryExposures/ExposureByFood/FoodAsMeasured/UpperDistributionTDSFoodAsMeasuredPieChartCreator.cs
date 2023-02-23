using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionTDSFoodAsMeasuredPieChartCreator : PieChartCreatorBase {

        private UpperDistributionTDSFoodAsMeasuredSection _section;

        public UpperDistributionTDSFoodAsMeasuredPieChartCreator(UpperDistributionTDSFoodAsMeasuredSection section) {
            Width = 500;
            Height = 350;
            _section = section;
            ExplodeFirstSlide = true;
        }

        public override string ChartId {
            get {
                var pictureId = "9810ece0-8490-4c1e-ab72-9a8391873fc6";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Contribution to upper tail distribution for TDS vs Read Across translations.";

        public override PlotModel Create() {
            var records = new List<TDSReadAcrossFoodRecord>();
            records.Add(_section.UpperDistributionTDSFoodAsMeasuredRecords.First());
            records.AddRange(_section.UpperDistributionTDSFoodAsMeasuredRecords.Skip(1).OrderByDescending(r => r.Contribution));
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

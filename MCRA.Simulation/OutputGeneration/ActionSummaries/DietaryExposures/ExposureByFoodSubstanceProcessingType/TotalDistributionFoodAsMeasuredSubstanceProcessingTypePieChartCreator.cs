using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionFoodAsMeasuredSubstanceProcessingTypePieChartCreator : ReportPieChartCreatorBase {

        private TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSection _section;
        private bool _isUncertainty;

        public TotalDistributionFoodAsMeasuredSubstanceProcessingTypePieChartCreator(TotalDistributionFoodAsMeasuredSubstanceProcessingTypeSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "d3ff9e03-6986-4707-a69a-cf3074ede18b";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Contribution to total exposure distribution for processed foods (modelled) and substances.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.FoodName,
                    r.SubstanceName,
                    r.ProcessingTypeName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(label: $"{r.FoodName}, {r.SubstanceName}, {r.ProcessingTypeName}", r.Contribution))
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
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

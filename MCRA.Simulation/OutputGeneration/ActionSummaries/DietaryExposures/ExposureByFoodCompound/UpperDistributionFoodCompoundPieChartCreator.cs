using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionFoodCompoundPieChartCreator : ReportPieChartCreatorBase {

        private UpperDistributionFoodCompoundSection _section;
        private bool _isUncertainty;

        public UpperDistributionFoodCompoundPieChartCreator(UpperDistributionFoodCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "5d250273-8551-4561-92dd-2a95e39ad7e4";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Contribution to the upper {_section.UpperPercentage:F1}% of the exposure distribution for modelled foods x substances (MSCC).";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.FoodName,
                    r.CompoundName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(label: $"{r.CompoundName} {r.FoodName}", r.Contribution))
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
            var palette = CustomPalettes.ArtDecoTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

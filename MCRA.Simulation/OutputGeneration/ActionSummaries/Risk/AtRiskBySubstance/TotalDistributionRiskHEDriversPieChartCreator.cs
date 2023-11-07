using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionRiskHEDriversPieChartCreator : PieChartCreatorBase {

        private HazardExposureRatioSubstanceSection _section;
        private bool _isUncertainty;

        public TotalDistributionRiskHEDriversPieChartCreator(HazardExposureRatioSubstanceSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "476ef9eb-a25b-44a5-bd8d-7cfd574b5f7a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Contribution of substances to total distribution.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.SubstanceName,
                    Contrib: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contrib > 0)
                .OrderByDescending(r => r.Contrib)
                .Select(r => new PieSlice(r.SubstanceName, r.Contrib))
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
            var palette = CustomPalettes.GorgeousToneReverse(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

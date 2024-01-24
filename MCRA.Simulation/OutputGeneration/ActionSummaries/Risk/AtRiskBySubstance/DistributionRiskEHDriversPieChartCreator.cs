using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DistributionRiskEHDriversPieChartCreator : ReportPieChartCreatorBase {

        private ExposureHazardRatioSubstanceSection _totalSection;
        private ExposureHazardRatioSubstanceUpperSection _upperSection;
        private bool _isUncertainty;
        private string _title;

        public DistributionRiskEHDriversPieChartCreator(
            ExposureHazardRatioSubstanceSection totalSection,
            ExposureHazardRatioSubstanceUpperSection upperSection, 
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _totalSection = totalSection;
            _upperSection = upperSection;
            _isUncertainty = isUncertainty;
            _title = _totalSection != null ? "total" : $"upper ({upperSection.CalculatedUpperPercentage.ToString("F1")}%)";
        }

        public override string ChartId {
            get {
                var pictureId = "476ef9eb-a25b-44a5-bd8d-7cfd574b5f7a";
                var sectionId = _totalSection != null ? _totalSection.SectionId : _upperSection.SectionId;
                return StringExtensions.CreateFingerprint(sectionId + pictureId);
            }
        }

        public override string Title => $"Contribution of substances to {_title} distribution.";

        public override PlotModel Create() {
            var records = _totalSection != null ? _totalSection.Records : _upperSection.Records;
            var pieSlices = records.Select(
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
        private PlotModel create(IEnumerable<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GorgeousToneReverse(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

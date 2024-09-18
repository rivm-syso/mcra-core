using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DistributionRiskDriversPieChartCreator : ReportPieChartCreatorBase {

        private readonly RiskRatioSubstanceSection _totalSection;
        private readonly RiskRatioSubstanceUpperSection _upperSection;
        private readonly bool _isUncertainty;
        private readonly string _title;
        private readonly bool _isPercentageAtRisk;

        public DistributionRiskDriversPieChartCreator(
            RiskRatioSubstanceSection totalSection,
            RiskRatioSubstanceUpperSection upperSection,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _totalSection = totalSection;
            _upperSection = upperSection;
            _isUncertainty = isUncertainty;
            _title = _totalSection != null ? "total distribution" : $"upper {upperSection.UpperPercentage:F1}% of the distribution";
            _isPercentageAtRisk = upperSection?.IsPercentageAtRisk ?? false;
        }

        public override string ChartId {
            get {
                var pictureId = "476ef9eb-a25b-44a5-bd8d-7cfd574b5f7a";
                var sectionId = _totalSection != null ? _totalSection.SectionId : $"{_upperSection.SectionId}{_isPercentageAtRisk}";
                return StringExtensions.CreateFingerprint(sectionId + pictureId);
            }
        }

        public override string Title => $"Contribution of substances to the {_title}.";

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
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

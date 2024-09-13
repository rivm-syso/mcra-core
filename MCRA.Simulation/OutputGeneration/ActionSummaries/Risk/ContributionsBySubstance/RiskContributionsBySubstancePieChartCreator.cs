using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class RiskContributionsBySubstancePieChartCreator : ReportPieChartCreatorBase {

        private readonly RiskContributionsBySubstanceSection _section;
        private readonly bool _isUncertainty;

        public RiskContributionsBySubstancePieChartCreator(
            RiskContributionsBySubstanceSection section,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public abstract override string Title { get; }

        public override string ChartId {
            get {
                var sectionId = _section.SectionId;
                var pictureId = "476ef9eb-a25b-44a5-bd8d-7cfd574b5f7a";
                return StringExtensions.CreateFingerprint(sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var records = _section.Records;
            var pieSlices = records
                .Select(r => (
                    Substance: r.SubstanceName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(r.Substance, r.Contribution))
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
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

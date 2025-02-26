using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySubstanceTotalPieChartCreator : ReportPieChartCreatorBase {

        private readonly ContributionBySubstanceTotalSection _section;
        private readonly bool _isUncertainty;

        public ContributionBySubstanceTotalPieChartCreator(
            ContributionBySubstanceTotalSection section,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "781bafb8-87e9-4a22-9786-f7ecbb7e7589";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Contribution to total exposure distribution for substances.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.SubstanceName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(r.SubstanceName, r.Contribution))
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
            var plotModel = create(pieSlices, noSlices);
            return plotModel;
        }
    }
}

using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualContributionsPieChartCreator : ReportPieChartCreatorBase {

        private ContributionsForIndividualsSection _section;
        private bool _isUncertainty;

        public IndividualContributionsPieChartCreator(
            ContributionsForIndividualsSection section,
            bool isUncertainty
        ) {
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

        public override string Title => $"Contribution to risks for individuals.";

        public override PlotModel Create() {
            var pieSlices = _section.IndividualContributionRecords.Select(
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
        private PlotModel create(IEnumerable<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices, minContributionFraction: 0.1);
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualContributionsUpperPieChartCreator : ReportPieChartCreatorBase {

        private ContributionsForIndividualsUpperSection _section;
        private bool _isUncertainty;
        bool _isPercentageAtRisk;
        public IndividualContributionsUpperPieChartCreator(
            ContributionsForIndividualsUpperSection section,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
            _isPercentageAtRisk = section.IsPercentageAtRisk;
        }

        public override string ChartId {
            get {
                var pictureId = "8f3a9b10-81e5-4f38-8bfc-749b44537a0f";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _isPercentageAtRisk);
            }
        }

        public override string Title => $"Mean contribution to risks for individuals to the " +
            $"upper {_section.UpperPercentage:F1}% of the distribution.";

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
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, 
        /// see custom model
        /// </summary>
        private PlotModel create(IEnumerable<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySubstanceUpperPieChartCreator : ReportPieChartCreatorBase {

        private readonly ContributionBySubstanceUpperSection _section;
        private readonly bool _isUncertainty;

        public ContributionBySubstanceUpperPieChartCreator(
            ContributionBySubstanceUpperSection section,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "ed192198-e5d4-47ab-ba25-ed3b603e6c40";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Contribution of substances to the upper {_section.UpperPercentage:F1}% of the exposure distribution.";

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

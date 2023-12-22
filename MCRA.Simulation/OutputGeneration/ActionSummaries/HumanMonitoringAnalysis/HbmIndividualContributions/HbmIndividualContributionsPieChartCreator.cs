using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualContributionsPieChartCreator : ReportPieChartCreatorBase {

        private HbmIndividualContributionsSection _section;
        private bool _isUncertainty;

        public HbmIndividualContributionsPieChartCreator(
            HbmIndividualContributionsSection section,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _isUncertainty = isUncertainty;
            _section = section;
        }

        public override string ChartId {
            get {
                var pictureId = "14a2b1f4-aaf3-46d6-b377-7dcaf406bb8e";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Contributions to cumulative concentrations for individuals.";

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
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

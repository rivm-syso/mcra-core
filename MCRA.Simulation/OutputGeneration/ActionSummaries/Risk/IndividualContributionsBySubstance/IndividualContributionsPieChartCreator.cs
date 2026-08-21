using MCRA.Simulation.OutputGeneration.ActionSummaries.Risk;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualContributionsPieChartCreator : ReportPieChartCreatorBase {

        private readonly List<RiskContributionBySubstanceRecord> _records;
        private readonly string _sectionId;
        private readonly bool _isUncertainty;

        public IndividualContributionsPieChartCreator(
            string sectionId,
            List<RiskContributionBySubstanceRecord> records,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _records = records;
            _sectionId = sectionId;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "476ef9eb-a25b-44a5-bd8d-7cfd574b5f7a";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override string Title => $"Mean contribution to risks for individuals.";

        public override PlotModel Create() {
            var pieSlices = _records.Select(
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
        private PlotModel create(IEnumerable<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

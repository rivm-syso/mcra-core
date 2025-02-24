using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionCompoundPieChartCreator : ReportPieChartCreatorBase {

        private TotalDistributionCompoundSection _section;
        private bool _isUncertainty;

        public TotalDistributionCompoundPieChartCreator(TotalDistributionCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "d594f048-1f63-429c-8ce5-4469da6c2555";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Contribution to total exposure distribution for substances.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.CompoundName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(r.CompoundName, r.Contribution))
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

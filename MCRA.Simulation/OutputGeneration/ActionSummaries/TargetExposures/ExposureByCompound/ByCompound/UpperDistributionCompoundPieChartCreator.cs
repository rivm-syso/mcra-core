using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionCompoundPieChartCreator : ReportPieChartCreatorBase {

        private UpperDistributionCompoundSection _section;
        private bool _isUncertainty;

        public UpperDistributionCompoundPieChartCreator(UpperDistributionCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "db03f99d-d876-49bf-999e-c39c74f59829";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Contribution of substances to the upper {_section.UpperPercentage:F1}% of the exposure distribution.";

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.Records.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records.Select(c => new PieSlice(c.CompoundName, c.MeanContribution)).ToList();
                return create(pieSlices);
            } else {
                var records = _section.Records.OrderByDescending(r => r.Contribution).ToList();
                var pieSlices = records.Select(c => new PieSlice(c.CompoundName, c.Contribution)).ToList();
                return create(pieSlices);
            }
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

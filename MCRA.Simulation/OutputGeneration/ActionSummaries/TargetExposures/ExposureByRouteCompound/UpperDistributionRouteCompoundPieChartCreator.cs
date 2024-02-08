using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionRouteCompoundPieChartCreator : ReportPieChartCreatorBase {

        private UpperDistributionRouteCompoundSection _section;
        private bool _isUncertainty;
        public UpperDistributionRouteCompoundPieChartCreator(UpperDistributionRouteCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "909fca07-e1fa-49f4-baf1-ebd0dda308c2";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Contribution by route x substance for the upper {_section.UpperPercentage}% of the exposure distribution.";

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.Records.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice($"{c.CompoundName}-{c.ExposureRoute}", c.MeanContribution))
                    .ToList();
                return create(pieSlices);
            } else {
                var records = _section.Records.OrderByDescending(r => r.Contribution).ToList();
                var pieSlices = records
                    .Where(r => r.Contribution > 0)
                    .Select(c => new PieSlice($"{c.CompoundName}-{c.ExposureRoute}", c.Contribution))
                    .ToList();
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

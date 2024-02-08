using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionAggregateRoutePieChartCreator : ReportPieChartCreatorBase {

        private UpperDistributionAggregateRouteSection _section;
        private bool _isUncertainty;
        public UpperDistributionAggregateRoutePieChartCreator(UpperDistributionAggregateRouteSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "6dbe61cd-7b07-45f8-9c20-01aa6df47660";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Contribution by route to the upper {_section.UpperPercentage}% of the exposure distribution.";

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.DistributionRouteUpperRecords.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice(c.ExposureRoute, c.MeanContribution))
                    .ToList();
                return create(pieSlices);
            } else {
                var records = _section.DistributionRouteUpperRecords.OrderByDescending(r => r.Contribution).ToList();
                var pieSlices = records
                    .Where(r => r.Contribution > 0)
                    .Select(c => new PieSlice(c.ExposureRoute, c.Contribution))
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

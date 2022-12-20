using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionAggregateRoutePieChartCreator : PieChartCreatorBase {

        private TotalDistributionAggregateRouteSection _section;
        private bool _isUncertainty;
        public TotalDistributionAggregateRoutePieChartCreator(TotalDistributionAggregateRouteSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "2d0e4c5c-d420-4728-aa06-09812c673bbe";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Contribution to total exposure distribution by route.";

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.DistributionRouteTotalRecords.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice(c.ExposureRoute, c.MeanContribution))
                    .ToList();
                return create(pieSlices);
            } else {
                var records = _section.DistributionRouteTotalRecords.OrderByDescending(r => r.Contribution).ToList();
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

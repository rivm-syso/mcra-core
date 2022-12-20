using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionRouteCompoundPieChartCreator : PieChartCreatorBase {

        private TotalDistributionRouteCompoundSection _section;
        private bool _isUncertainty;
        public TotalDistributionRouteCompoundPieChartCreator(TotalDistributionRouteCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "6b013173-b2fb-405e-a260-0ecab1c39e76";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Contribution to total exposure distribution for route x substance.";

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

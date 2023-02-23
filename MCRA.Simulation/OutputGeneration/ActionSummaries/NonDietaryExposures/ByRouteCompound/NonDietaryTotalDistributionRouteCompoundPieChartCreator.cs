using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryTotalDistributionRouteCompoundPieChartCreator : PieChartCreatorBase {

        private NonDietaryTotalDistributionRouteCompoundSection _section;
        private bool _isUncertainty;
        public NonDietaryTotalDistributionRouteCompoundPieChartCreator(NonDietaryTotalDistributionRouteCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string Title => "Contribution to the total exposure distribution by route x substance";

        public override string ChartId {
            get {
                var pictureId = "4f466ded-467a-44e2-9df5-f30c6ea1579e";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.NonDietaryTotalDistributionRouteCompoundRecords.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice($"{c.CompoundName}-{c.ExposureRoute}", c.MeanContribution))
                    .ToList();
                return create(pieSlices);
            } else {
                var records = _section.NonDietaryTotalDistributionRouteCompoundRecords.OrderByDescending(r => r.Contribution).ToList();
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
            var plotModel = base.create(pieSlices, noSlices);
            return plotModel;
        }
    }
}

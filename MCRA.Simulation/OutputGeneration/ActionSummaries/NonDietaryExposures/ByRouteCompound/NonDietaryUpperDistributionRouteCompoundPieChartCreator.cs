using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryUpperDistributionRouteCompoundPieChartCreator : ReportPieChartCreatorBase {

        private NonDietaryUpperDistributionRouteCompoundSection _section;
        private bool _isUncertainty;
        public NonDietaryUpperDistributionRouteCompoundPieChartCreator(NonDietaryUpperDistributionRouteCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string Title => $"Contribution by route x substance to the upper ({_section.UpperPercentage:F1}%) of the exposure distribution.";

        public override string ChartId {
            get {
                var pictureId = "6f3f8b850-52ce-4278-8374-f7e7f0bd5342";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.NonDietaryUpperDistributionRouteCompoundRecords.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice($"{c.CompoundName}-{c.ExposureRoute}", c.MeanContribution))
                    .ToList();
                return create(pieSlices);
            } else {
                var records = _section.NonDietaryUpperDistributionRouteCompoundRecords.OrderByDescending(r => r.Contribution).ToList();
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

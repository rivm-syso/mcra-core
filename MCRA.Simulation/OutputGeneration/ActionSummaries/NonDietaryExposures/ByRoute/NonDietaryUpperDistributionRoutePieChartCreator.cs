using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryUpperDistributionRoutePieChartCreator : PieChartCreatorBase {

        private NonDietaryUpperDistributionRouteSection _section;
        private bool _isUncertainty;
        public NonDietaryUpperDistributionRoutePieChartCreator(NonDietaryUpperDistributionRouteSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string Title => "Contribution to the upper exposure distribution by route";

        public override string ChartId {
            get {
                var pictureId = "8b5f51ec-9d8c-4390-9f45-728084abb2df";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.NonDietaryUpperDistributionRouteRecords.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice(label: $"{c.ExposureRoute}", c.MeanContribution))
                    .ToList();
                return create(pieSlices);
            } else {
                var records = _section.NonDietaryUpperDistributionRouteRecords.OrderByDescending(r => r.Contribution).ToList();
                var pieSlices = records
                    .Where(r => r.Contribution > 0)
                    .Select(c => new PieSlice(label: $"{c.ExposureRoute}", c.Contribution))
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
            var palette = CustomPalettes.CoolTone(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

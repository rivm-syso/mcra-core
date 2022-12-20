using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionFoodCompoundPieChartCreator : PieChartCreatorBase {

        private UpperDistributionFoodCompoundSection _section;
        private bool _isUncertainty;

        public UpperDistributionFoodCompoundPieChartCreator(UpperDistributionFoodCompoundSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "5d250273-8551-4561-92dd-2a95e39ad7e4";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Contribution to the upper tail distribution for modelled foods x substances (MSCC).";

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.Records.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records.Select(c => new PieSlice(label: $"{c.CompoundName} {c.FoodName}", c.MeanContribution)).ToList();
                return create(pieSlices);
            } else {
                var records = _section.Records.OrderByDescending(r => r.Contribution).ToList();
                var pieSlices = records.Select(c => new PieSlice(label: $"{c.CompoundName} {c.FoodName}", c.Contribution)).ToList();
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
            var palette = CustomPalettes.ArtDecoTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

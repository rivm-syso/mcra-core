﻿using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionFoodAsMeasuredPieChartCreator : ReportPieChartCreatorBase {

        private TotalDistributionFoodAsMeasuredSection _section;
        private List<DistributionFoodRecord> _records;
        private bool _isUncertainty;
        private int _counter;

        public TotalDistributionFoodAsMeasuredPieChartCreator(
            TotalDistributionFoodAsMeasuredSection section,
            List<DistributionFoodRecord> records,
            bool isUncertainty,
            int counter = 0
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _records = records;
            _isUncertainty = isUncertainty;
            _counter = counter;
        }

        public override string ChartId {
            get {
                var pictureId = "fa576c27-6173-4711-ba74-8ba49d41915a";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _counter);
            }
        }

        public override string Title => "Contribution to total exposure distribution for modelled foods.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.FoodName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(r.FoodName, r.Contribution))
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
            var palette = CustomPalettes.GreenToneReverse(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

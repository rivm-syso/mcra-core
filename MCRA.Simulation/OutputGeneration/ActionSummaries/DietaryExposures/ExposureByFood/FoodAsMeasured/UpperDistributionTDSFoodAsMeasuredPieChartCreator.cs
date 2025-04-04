﻿using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionTDSFoodAsMeasuredPieChartCreator : ReportPieChartCreatorBase {

        private UpperDistributionTDSFoodAsMeasuredSection _section;

        public UpperDistributionTDSFoodAsMeasuredPieChartCreator(UpperDistributionTDSFoodAsMeasuredSection section) {
            Width = 500;
            Height = 350;
            _section = section;
            ExplodeFirstSlide = true;
        }

        public override string ChartId {
            get {
                var pictureId = "9810ece0-8490-4c1e-ab72-9a8391873fc6";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Contribution for TDS vs Read Across translations to the upper {_section.UpperPercentage:F1}% of the exposure distribution.";


        public override PlotModel Create() {
            var records = new List<TDSReadAcrossFoodRecord> {
                _section.Records.First()
            };
            records.AddRange(_section.Records.Skip(1).OrderByDescending(r => r.Contribution));
            var pieSlices = records.Select(c => new PieSlice(c.FoodName, c.Contribution)).ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.BeachTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

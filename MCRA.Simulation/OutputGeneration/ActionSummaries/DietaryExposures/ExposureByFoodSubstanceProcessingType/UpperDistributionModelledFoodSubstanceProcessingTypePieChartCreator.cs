﻿using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UpperDistributionModelledFoodSubstanceProcessingTypePieChartCreator : ReportPieChartCreatorBase {

        private UpperDistributionModelledFoodSubstanceProcessingTypeSection _section;
        private bool _isUncertainty;

        public UpperDistributionModelledFoodSubstanceProcessingTypePieChartCreator(UpperDistributionModelledFoodSubstanceProcessingTypeSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "fc422beb-4f4b-400d-a22a-a6263d6d9e5f";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Contribution for processed foods (modelled) and substances to the upper {_section.UpperPercentage:F1}% of the exposure distribution.";

        public override PlotModel Create() {
            if (_isUncertainty) {
                var records = _section.Records.OrderByDescending(r => r.MeanContribution).ToList();
                var pieSlices = records
                    .Where(r => r.MeanContribution > 0)
                    .Select(c => new PieSlice($"{c.FoodName}, {c.SubstanceName}, {c.ProcessingTypeName}", c.MeanContribution))
                    .ToList();
                return create(pieSlices);
            } else {
                var records = _section.Records.OrderByDescending(r => r.Contribution).ToList();
                var pieSlices = records
                    .Where(r => r.Contribution > 0)
                    .Select(c => new PieSlice($"{c.FoodName}, {c.SubstanceName}, {c.ProcessingTypeName}", c.Contribution))
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
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

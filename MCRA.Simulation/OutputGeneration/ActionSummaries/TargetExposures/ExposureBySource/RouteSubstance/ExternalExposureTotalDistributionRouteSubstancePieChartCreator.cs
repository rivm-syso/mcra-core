﻿using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExternalExposureTotalDistributionRouteSubstancePieChartCreator : ReportPieChartCreatorBase {

        private ExternalExposureTotalDistributionRouteSubstanceSection _section;
        private bool _isUncertainty;
        public ExternalExposureTotalDistributionRouteSubstancePieChartCreator(ExternalExposureTotalDistributionRouteSubstanceSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string Title => "Contribution to the total exposure distribution by route x substance.";

        public override string ChartId {
            get {
                var pictureId = "4f466ded-467a-44e2-9df5-f30c6ea1579e";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.ExposureRoute,
                    r.SubstanceName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice($"{r.SubstanceName}-{r.ExposureRoute}", r.Contribution))
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
            var palette = CustomPalettes.BeachToneReverse(noSlices);
            var plotModel = base.create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

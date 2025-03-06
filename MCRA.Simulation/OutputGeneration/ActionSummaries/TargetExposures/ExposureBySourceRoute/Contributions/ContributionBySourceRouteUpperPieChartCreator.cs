using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySourceRouteUpperPieChartCreator : ReportPieChartCreatorBase {

        private readonly ContributionBySourceRouteUpperSection _section;
        private readonly bool _isUncertainty;
        public ContributionBySourceRouteUpperPieChartCreator(ContributionBySourceRouteUpperSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "3528c725-894c-4b37-9a63-a727c2f84467";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Contribution by source and route to the upper {_section.UpperPercentage:F1}% of the exposure distribution.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.ExposureRoute,
                    r.ExposureSource,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice($"{r.ExposureSource} {r.ExposureRoute}", r.Contribution))
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
            var palette = CustomPalettes.DistinctTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

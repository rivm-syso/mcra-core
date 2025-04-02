using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySourceRouteSubstanceTotalPieChartCreator : ReportPieChartCreatorBase {

        private readonly ContributionBySourceRouteSubstanceTotalSection _section;
        private readonly bool _isUncertainty;
        public ContributionBySourceRouteSubstanceTotalPieChartCreator(ContributionBySourceRouteSubstanceTotalSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "c63f6c51-c9d4-440a-9206-7357f526568f";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Contribution to total exposure distribution by source, route and substance.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.ExposureSource,
                    r.ExposureRoute,
                    r.SubstanceName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice($"{r.ExposureSource} {r.ExposureRoute} {r.SubstanceName}", r.Contribution))
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

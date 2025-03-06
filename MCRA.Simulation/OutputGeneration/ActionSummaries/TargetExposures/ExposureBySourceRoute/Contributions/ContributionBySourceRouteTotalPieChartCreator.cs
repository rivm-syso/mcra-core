using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionBySourceRouteTotalPieChartCreator : ReportPieChartCreatorBase {

        private readonly ContributionBySourceRouteTotalSection _section;
        private readonly bool _isUncertainty;
        public ContributionBySourceRouteTotalPieChartCreator(ContributionBySourceRouteTotalSection section, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "dedbf6fc-1049-4e4c-9531-c2e3603b8a57";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => "Contribution to total exposure distribution by source and route.";

        public override PlotModel Create() {
            var pieSlices = _section.Records.Select(
                r => (
                    r.ExposureSource,
                    r.ExposureRoute,
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

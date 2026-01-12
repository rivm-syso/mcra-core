using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute {
    public class ExternalExposuresByRoutePieChartCreator : ReportPieChartCreatorBase {

        private readonly ExternalExposuresByRouteSection _section;
        private readonly bool _isUncertainty;

        public ExternalExposuresByRoutePieChartCreator(
            ExternalExposuresByRouteSection section,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                return StringExtensions.CreateFingerprint(_section.SectionId + _section.PictureId);
            }
        }

        public override string Title => "Contribution to total exposure distribution for routes.";

        public override PlotModel Create() {
            var pieSlices = _section.ExposureRecords.Select(
                r => (
                    r.ExposureRoute,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(r.ExposureRoute, r.Contribution))
                .ToList();
            return create(pieSlices);
        }

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.BeachTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

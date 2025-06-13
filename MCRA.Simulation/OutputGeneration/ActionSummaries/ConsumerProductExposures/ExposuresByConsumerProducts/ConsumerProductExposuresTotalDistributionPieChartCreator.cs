using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposuresTotalDistributionPieChartCreator : ReportPieChartCreatorBase {

        private readonly ConsumerProductExposuresTotalDistributionSection _section;
        private readonly List<ConsumerProductExposureRecord> _records;
        private readonly bool _isUncertainty;
        private readonly int _counter;

        public ConsumerProductExposuresTotalDistributionPieChartCreator(
            ConsumerProductExposuresTotalDistributionSection section,
            List<ConsumerProductExposureRecord> records,
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
                var pictureId = "45142be4-4274-4869-8200-f8cde245c275";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _counter);
            }
        }

        public override string Title => "Contribution to total exposure distribution for consumer products.";

        public override PlotModel Create() {
            var pieSlices = _records.Select(
                r => (
                    r.ConsumerProductName,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(r.ConsumerProductName, r.Contribution))
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

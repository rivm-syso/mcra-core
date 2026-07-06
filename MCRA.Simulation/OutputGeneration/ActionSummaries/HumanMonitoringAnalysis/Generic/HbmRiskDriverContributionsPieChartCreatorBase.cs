using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmRiskDriverContributionsPieChartCreatorBase<S, T> : ReportPieChartCreatorBase
        where S : IHbmExposureContributorKey, new()
        where T : HbmContributionRecordBase<S>, new() {

        private readonly string _sectionId;
        private readonly bool _isUncertainty;
        private readonly List<T> _records;

        public HbmRiskDriverContributionsPieChartCreatorBase(string sectionId, List<T> records, bool isUncertainty) {
            Width = 500;
            Height = 350;
            _sectionId = sectionId;
            _records = records;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "4c761248-fd8a-4ed5-b616-c65f8f084b95";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var pieSlices = _records
                .Where(c => string.IsNullOrEmpty(c.Stratification))
                .Select(r => (
                    SubstanceName: r.GetDescriptorKey(),
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .OrderByDescending(r => r.Contribution)
                .Select(r => new PieSlice(r.SubstanceName, r.Contribution))
                .ToList();
            return create(pieSlices);
        }

        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = getNumberOfSlices(pieSlices);
            var palette = CustomPalettes.GorgeousTone(noSlices);
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

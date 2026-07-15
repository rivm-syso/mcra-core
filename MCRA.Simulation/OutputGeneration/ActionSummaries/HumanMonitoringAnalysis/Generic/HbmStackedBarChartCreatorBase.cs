using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmStackedBarChartCreatorBase<S, T> : ReportStackedBarChartCreatorBase
        where S : IExposureContributorKey, new()
        where T : HbmContributionRecordBase<S>, new() {

        private string _sectionId;
        private readonly List<T> _records;
        private readonly bool _isUncertainty;

        public HbmStackedBarChartCreatorBase(
            string sectionId,
            List<T> records,
            bool isUncertainty
        ) {
            Width = 500;
            Height = 350;
            _records = records;
            _sectionId = sectionId;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "653931e8-6977-457e-ba44-31b74f6ef69b";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var barDatapoints = _records
                .Select(r => (
                    Descriptor: r.GetDescriptorKey(),
                    Stratifier: r.Stratification,
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .Select(r => new BarDataPoint(r.Stratifier, r.Descriptor, r.Contribution))
                .ToList();
            return create(barDatapoints);
        }
    }
}

using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposureStackedBarChartCreator<S, T> : ReportStackedBarChartCreatorBase 
        where S : IExposureContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly string _stratification;
        private readonly bool _isUncertainty;
        private readonly bool _isUpper;
        private readonly string _descriptorKey;

        public InternalExposureStackedBarChartCreator(
            List<T> records,
            string stratification,
            bool isUncertainty,
            string descriptorKey,
            bool isUpper = false
        ) {
            Width = 500;
            Height = 350;
            _records = records;
            _isUncertainty = isUncertainty;
            _stratification = stratification;
            _isUpper = isUpper;
            _descriptorKey = descriptorKey;
        }

        public override string ChartId {
            get {
                var pictureId = "baf521df-1aa5-44ea-b999-7dce0cda89cd";
                return StringExtensions.CreateFingerprint(_descriptorKey + _stratification + pictureId + _isUpper.ToString());
            }
        }

        public override string Title => _isUpper
            ? "Contribution by route for the upper exposure distribution."
            : "Contribution by route for the total exposure distribution.";

        public override PlotModel Create() {
            var barDatapoints = _records.Select(
                r => (
                    Descriptor: r.GetKey(),
                    Stratifier: r.Stratification,
                    Contribution: _isUncertainty ? r.MeanContribution / 100 : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .Select(r => new BarDataPoint(r.Stratifier, r.Descriptor, r.Contribution * 100))
                .ToList();
            return create(barDatapoints);
        }
    }
}

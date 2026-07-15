using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class InternalExposureStackedBarChartCreator<S, T> : ReportStackedBarChartCreatorBase 
        where S : IContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new() {

        protected readonly List<T> _records;
        protected readonly bool _isUncertainty;
        protected readonly string _descriptorName;

        public InternalExposureStackedBarChartCreator(
            List<T> records,
            bool isUncertainty,
            string descriptorName
        ) {
            Width = 500;
            Height = 350;
            _records = records;
            _isUncertainty = isUncertainty;
            _descriptorName = descriptorName;
        }

        public override string ChartId {
            get {
                var pictureId = "baf521df-1aa5-44ea-b999-7dce0cda89cd";
                return StringExtensions.CreateFingerprint(_descriptorName + pictureId);
            }
        }

        public override string Title => $"Contribution by {_descriptorName} for the total exposure distribution.";

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

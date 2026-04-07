using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposureUpperStackedBarChartCreator<S, T> : InternalExposureStackedBarChartCreator<S, T>
        where S : IExposureContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new() {

        public InternalExposureUpperStackedBarChartCreator(
            List<T> records,
            bool isUncertainty,
            string descriptorName
         ) : base(records, isUncertainty, descriptorName) {
            Width = 500;
            Height = 350;
        }

        public override string ChartId {
            get {
                var pictureId = "c478674c-0a5d-4434-8d64-77b0554b92cd";
                return StringExtensions.CreateFingerprint(_descriptorName + pictureId);
            }
        }

        public override string Title =>  $"Contribution by {_descriptorName} for the upper exposure distribution.";

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

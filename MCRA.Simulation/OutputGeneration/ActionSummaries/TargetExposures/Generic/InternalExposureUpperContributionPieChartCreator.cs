using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposureUpperContributionPieChartCreator<S, T> : InternalExposureContributionPieChartCreator<S, T>
        where S : IExposureContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new() {

        public InternalExposureUpperContributionPieChartCreator(
            List<T> records,
            bool isUncertainty,
            string descriptorName
        ) : base(records, isUncertainty, descriptorName) {
            Width = 500;    
            Height = 350;
        }

        public override string ChartId {
            get {
                var pictureId = "ae6bb4c4-c068-496a-8b1b-b87800a1443f";
                return StringExtensions.CreateFingerprint(_descriptorName + pictureId);
            }
        }
        public override string Title => $"Contribution by {_descriptorName} for the upper exposure distribution.";

        public override PlotModel Create() {
            var pieSlices = _records.Select(
                r => (
                    Descriptor: r.GetKey(),
                    Contribution: _isUncertainty ? r.MeanContribution : r.Contribution
                ))
                .Where(r => r.Contribution > 0)
                .Select(r => new PieSlice(label: $"{r.Descriptor}", r.Contribution))
                .ToList();
            return create(pieSlices);
        }
    }
}

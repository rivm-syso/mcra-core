using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalExposureContributionPieChartCreator<S, T> : ReportPieChartCreatorBase
        where S : IExposureContributorKey, new()
        where T : InternalExposureContributionRecordBase<S>, new() {

        private readonly List<T> _records;
        private readonly bool _isUncertainty;
        private readonly bool _isUpper;
        private readonly string _descriptorName;
        public InternalExposureContributionPieChartCreator(
            List<T> records,
            bool isUncertainty,
            string descriptorName,
            bool isUpper = false
        ) {
            Width = 500;
            Height = 350;
            _records = records;
            _isUncertainty = isUncertainty;
            _isUpper = isUpper;
            _descriptorName = descriptorName;
        }

        public override string ChartId {
            get {
                var pictureId = "4a1262b6-ad55-4537-91dd-baa518e90927";
                return StringExtensions.CreateFingerprint(_descriptorName + pictureId + _isUpper.ToString());
            }
        }
        public override string Title => _isUpper
            ? $"Contribution by {_descriptorName} for the upper exposure distribution."
            : $"Contribution by {_descriptorName} for the total exposure distribution.";

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

        /// <summary>
        /// To add a legenda, set plotmodel IsLegendVisible = true, and add an empty Title for the series, see custom model
        /// </summary>
        /// <param name="pieSlices"></param>
        /// <returns></returns>
        private PlotModel create(List<PieSlice> pieSlices) {
            var noSlices = pieSlices.Count;
            var palette = new OxyPalette(CustomPalettes.SplitComplementary(noSlices, 0.5883, .3, .3, .9, .9)
                .Colors
                .Select(c => OxyColor.FromAColor(175, c))
                .ToList()
            );
            var plotModel = create(pieSlices, noSlices, palette);
            return plotModel;
        }
    }
}

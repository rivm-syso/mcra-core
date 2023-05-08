using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks {
    public sealed class AFDensityChartCreator : AFDensityChartCreatorBase {

        public AFDensityChartCreator(SingleValueRisksAdjustmentFactorsSection section, bool isExposure)
            : base(section, isExposure) {
        }

        public override string ChartId {
            get {
                if (_isExposure) {
                    var pictureId = "18834b1d-04b1-4a04-b20b-678fb1365278";
                    return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
                } else {
                    var pictureId = "57cba7bc-3b97-49de-9825-05f77ba99ff3";
                    return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
                }
            }
        }
        public override string Title => _title;

        public override PlotModel Create() {
            return CreateDensity();
        }
    }
}

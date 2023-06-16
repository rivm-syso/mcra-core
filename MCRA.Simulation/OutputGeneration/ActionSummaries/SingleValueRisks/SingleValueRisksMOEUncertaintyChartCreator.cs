using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksMOEUncertaintyChartCreator : SingleValueRisksUncertaintyChartCreatorBase {

        public SingleValueRisksMOEUncertaintyChartCreator(SingleValueRisksThresholdExposureRatioSection section)
            : base(section) {
        }

        public override string ChartId {
            get {
                var pictureId = "fd882ab8-8203-4f62-8961-c92e55deb4aa";
                return StringExtensions.CreateFingerprint(_thresholdExposureSection.SectionId + pictureId);
            }
        }
        public override string Title => $"{_title}";

        public override PlotModel Create() {
            return CreateBoxPlot();
        }
    }
}



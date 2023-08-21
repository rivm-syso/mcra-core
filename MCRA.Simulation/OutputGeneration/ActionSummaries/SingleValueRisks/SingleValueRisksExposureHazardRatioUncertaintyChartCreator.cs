using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksExposureHazardRatioUncertaintyChartCreator : SingleValueRisksUncertaintyChartCreatorBase {

        public SingleValueRisksExposureHazardRatioUncertaintyChartCreator(SingleValueRisksExposureHazardRatioSection section)
            : base(section) {
        }

        public override string ChartId {
            get {
                var pictureId = "90f5c49b-ef74-49b2-b2e3-01f8b8989e13";
                return StringExtensions.CreateFingerprint(_exposureHazardSection.SectionId + pictureId);
            }
        }
        public override string Title => _title;

        public override PlotModel Create() {
            return CreateBoxPlot();
        }
    }
}



using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationModelDetectsMatrixViewChartCreator : ConcentrationModelChartCreatorBase {

        public ConcentrationModelDetectsMatrixViewChartCreator(ConcentrationModelRecord record, int height, int width, bool showTitle = false)
            : base(record, height, width, showTitle) {
        }

        public override string ChartId {
            get {
                var pictureId = "97052f9c-206f-4bcf-bbc0-7c635032201b";
                return StringExtensions.CreateFingerprint(_record.Id + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_record, _showTitle, true, false);
        }
    }
}

using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationModelNonDetectsMatrixViewChartCreator : ConcentrationModelChartCreatorBase {

        public ConcentrationModelNonDetectsMatrixViewChartCreator(ConcentrationModelRecord record, int height, int width, bool showTitle = false)
            : base(record, height, width, showTitle) {
        }

        public override string ChartId {
            get {
                var pictureId = "f196789e-24e6-48f3-b1f0-4704eeab8320";
                return StringExtensions.CreateFingerprint(_record.Id + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_record, _showTitle, false, true);
        }
    }
}

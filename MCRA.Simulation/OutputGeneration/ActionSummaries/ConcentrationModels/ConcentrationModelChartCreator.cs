using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationModelChartCreator : ConcentrationModelChartCreatorBase {

        public ConcentrationModelChartCreator(ConcentrationModelRecord record, int height, int width, bool showTitle) 
            : base(record, height, width, showTitle) {
        }

        public override string ChartId {
            get {
                var pictureId = "03616C31-78A0-4A83-BADE-47EA8530EEAC";
                return StringExtensions.CreateFingerprint(_record.Id + pictureId);
            }
        }

        public override PlotModel Create() {
            if (_record.FractionPositives > 0 || _record.Model != ConcentrationModelType.Empirical) {
                return create(_record, _showTitle, false, true);
            } else {
                return createCensoredValuesBarSeries(_record);
            }
        }
    }
}

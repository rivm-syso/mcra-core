using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductConcentrationModelChartCreator : ConsumerProductConcentrationModelChartCreatorBase {

        public ConsumerProductConcentrationModelChartCreator(
            ConsumerProductConcentrationModelRecord record,
            int height,
            int width,
            bool showTitle
        ) : base(record, height, width, showTitle) {
        }

        public override string ChartId {
            get {
                var pictureId = "daef52c7-c380-4345-af36-767eb24ffd40";
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

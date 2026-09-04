using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductPercentilesFitChartCreator : CumulativeChartCreatorBase {

        private readonly ParameterRecord _parameterRecord;
        private readonly string _intakeUnit;
        private readonly string _sectionId;

        public ConsumerProductPercentilesFitChartCreator(string sectionid, ParameterRecord parameterRecord, string intakeUnit) {
            Width = 500;
            Height = 350;
            _sectionId = sectionid + parameterRecord.ExposureSetCode;
            _parameterRecord = parameterRecord;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "fed04b18-86bf-41bd-bc65-ccd63401a0bf";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }
        public override string Title => $"Cumulative consumer product exposure distribution.";

        public override PlotModel Create() {
            return createPlotModel(
                _parameterRecord.Percentiles,
                _parameterRecord.FittedValues,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}

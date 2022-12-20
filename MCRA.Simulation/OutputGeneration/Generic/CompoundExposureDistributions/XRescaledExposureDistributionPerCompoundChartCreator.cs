using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class XRescaledExposureDistributionPerCompoundChartCreator : CompoundExposureDistributionChartCreatorBase {
        private CompoundExposureDistributionRecord _record;
        private string _intakeUnit;
        private double _maximum;
        private double _minimum;

        public XRescaledExposureDistributionPerCompoundChartCreator(CompoundExposureDistributionRecord record, int width, int height, double maximum, double minimum, string intakeUnit) {
            Width = width;
            Height = height;
            _record = record;
            _intakeUnit = intakeUnit;
            _minimum = minimum;
            _maximum = maximum;
        }

        public override string ChartId {
            get {
                var pictureId = "88bff761-4b27-4dba-86c9-c5de91437563";
                return StringExtensions.CreateFingerprint(_record.Id + pictureId);
            }
        }

        public override PlotModel Create() {
            var title = $"{_record.CompoundName} {100 - _record.Percentage:F1}% > 0";
            return Create(_record, title, _intakeUnit, _maximum, _minimum, true);
        }
    }
}

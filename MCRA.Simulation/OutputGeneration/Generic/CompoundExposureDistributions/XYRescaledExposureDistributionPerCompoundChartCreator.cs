using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class XYRescaledExposureDistributionPerCompoundChartCreator : CompoundExposureDistributionChartCreatorBase {
        private SubstanceExposureDistributionRecord _record;
        private string _intakeUnit;
        private double _maximum;
        private double _minimum;
        private double _maximumFrequency;

        public XYRescaledExposureDistributionPerCompoundChartCreator(SubstanceExposureDistributionRecord record, int width, int height, double maximum, double minimum, double maximumFrequency, string intakeUnit) {
            Width = width;
            Height = height;
            _record = record;
            _intakeUnit = intakeUnit;
            _minimum = minimum;
            _maximum = maximum;
            _maximumFrequency = maximumFrequency;
        }


        public override string Title => $"{_record.SubstanceName} {100 - _record.Percentage:F1}% > 0";
        public override string ChartId {
            get {
                var pictureId = "209af570-aa13-455d-b15f-82e4fc5cabd0";
                return StringExtensions.CreateFingerprint(_record.Id + pictureId);
            }
        }

        public override PlotModel Create() {
            return Create(_record, string.Empty, _intakeUnit, _maximum, _minimum, _maximumFrequency, true);
        }
    }
}

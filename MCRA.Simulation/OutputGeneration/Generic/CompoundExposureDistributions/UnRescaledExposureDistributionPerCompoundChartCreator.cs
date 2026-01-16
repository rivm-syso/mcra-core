using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UnrescaledExposureDistributionPerCompoundChartCreator : CompoundExposureDistributionChartCreatorBase {
        private SubstanceExposureDistributionRecord _record;
        private string _intakeUnit;

        public UnrescaledExposureDistributionPerCompoundChartCreator(SubstanceExposureDistributionRecord record, int width, int height, string intakeUnit) {
            Width = width;
            Height = height;
            _record = record;
            _intakeUnit = intakeUnit;
        }
        public override string Title => $"{_record.SubstanceName} {100 - _record.Percentage:F1}% > 0";
        public override string ChartId {
            get {
                var pictureId = "9391a2d6-36e8-4b44-9dbf-f464c558a902";
                return StringExtensions.CreateFingerprint(_record.Id + pictureId);
            }
        }

        public override PlotModel Create() {
            return Create(_record, string.Empty, _intakeUnit, true);
        }
    }
}

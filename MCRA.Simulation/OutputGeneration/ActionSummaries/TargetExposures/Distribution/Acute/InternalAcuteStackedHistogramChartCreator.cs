using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalAcuteStackedHistogramChartCreator : StackedHistogramChartCreatorBase {

        private readonly InternalDistributionTotalSection _section;
        private readonly string _intakeUnit;

        public InternalAcuteStackedHistogramChartCreator(InternalDistributionTotalSection section, string intakeUnit) {
            ShowContributions = false;
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "41f251e7-09a4-48ae-bf9a-ec7685a998b8";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Transformed exposure distribution  ({100 - _section.PercentageZeroIntake:F1} % positives).";

        public override PlotModel Create() {
            return create(
                _section.CategorizedHistogramBins,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}
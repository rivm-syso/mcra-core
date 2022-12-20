using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryTotalIntakeCoExposureCumulativeDistributionChartCreator : CumulativeLineChartCreatorBase {

        private DietaryTotalIntakeCoExposureDistributionSection _section;
        private string _intakeUnit;

        public DietaryTotalIntakeCoExposureCumulativeDistributionChartCreator(DietaryTotalIntakeCoExposureDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "9299ed73-ee86-4833-9ca7-696d0c14c852";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Cumulative dietary exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives).";

        public override PlotModel Create() {
            return createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerlimit,
                _section.UncertaintyUpperlimit,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}

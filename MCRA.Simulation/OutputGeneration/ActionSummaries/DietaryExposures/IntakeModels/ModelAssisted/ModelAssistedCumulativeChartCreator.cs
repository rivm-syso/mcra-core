using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelAssistedCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private ModelAssistedDistributionSection _section;
        private string _intakeUnit;

        public ModelAssistedCumulativeChartCreator(ModelAssistedDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "c9392e1d-8902-45e1-bd67-2f866cd829f3";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Model assisted cumulative exposure distribution.";

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                _intakeUnit
            );
        }
    }
}

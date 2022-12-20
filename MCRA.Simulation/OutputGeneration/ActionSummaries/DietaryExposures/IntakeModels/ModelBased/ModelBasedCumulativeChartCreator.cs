using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelBasedCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private ModelBasedDistributionSection _section;
        private string _intakeUnit;

        public ModelBasedCumulativeChartCreator(ModelBasedDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }
        public override string Title => "Model based cumulative exposure distribution";

        public override string ChartId {
            get {
                var pictureId = "c19b1c1c-72cc-425e-966b-e2fc7d5ad561";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return base.createPlotModel(
                _section.Percentiles,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}

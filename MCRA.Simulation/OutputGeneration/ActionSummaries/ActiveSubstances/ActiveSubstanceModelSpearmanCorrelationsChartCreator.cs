using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstanceModelSpearmanCorrelationsChartCreator : ActiveSubstanceModelCorrelationsChartCreatorBase {

        public ActiveSubstanceModelSpearmanCorrelationsChartCreator(ActiveSubstanceModelCorrelationsSection section)
            : base(section) {
        }

        public override string Title => "Spearman rank correlations chart of the active substance models.";

        public override string ChartId {
            get {
                var pictureId = "9448AEA8-253A-4285-A6AA-494A6EF6FAD9";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var correlations = new double[_section.SpearmanCorrelations.Count, _section.SpearmanCorrelations.Count];
            for (int i = 0; i < _section.SpearmanCorrelations.Count; i++) {
                for (int j = 0; j < _section.SpearmanCorrelations.Count; j++) {
                    correlations[i, j] = _section.SpearmanCorrelations[i][j];
                }
            }
            return create(correlations, _section.ModelNames);
        }
    }
}

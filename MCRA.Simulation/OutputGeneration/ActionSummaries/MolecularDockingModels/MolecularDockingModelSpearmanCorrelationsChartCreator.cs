using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelSpearmanCorrelationsChartCreator : MolecularDockingModelCorrelationsChartCreatorBase {

        public MolecularDockingModelSpearmanCorrelationsChartCreator(MolecularDockingModelCorrelationsSummarySection section)
            : base(section) {
        }

        public override string ChartId {
            get {
                var pictureId = "26D67685-8C44-4573-A5C8-D5F103D30A6A";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Spearman rank correlations chart of the molecular docking models.";

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

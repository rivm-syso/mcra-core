using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MolecularDockingModelPearsonCorrelationsChartCreator : MolecularDockingModelCorrelationsChartCreatorBase {

        public override string ChartId {
            get {
                var pictureId = "EF34F38E-4D8F-421F-8782-005EE37A6C2A";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Pearson correlations chart of the molecular docking models.";

        public MolecularDockingModelPearsonCorrelationsChartCreator(MolecularDockingModelCorrelationsSummarySection section)
            : base(section) {
        }

        public override PlotModel Create() {
            var correlations = new double[_section.PearsonCorrelations.Count, _section.PearsonCorrelations.Count];
            for (int i = 0; i < _section.PearsonCorrelations.Count; i++) {
                for (int j = 0; j < _section.PearsonCorrelations.Count; j++) {
                    correlations[i, j] = _section.PearsonCorrelations[i][j];
                }
            }
            return create(correlations, _section.ModelNames);
        }
    }
}

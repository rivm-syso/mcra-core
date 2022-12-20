using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstanceModelPearsonCorrelationsChartCreator : ActiveSubstanceModelCorrelationsChartCreatorBase {

        public ActiveSubstanceModelPearsonCorrelationsChartCreator(ActiveSubstanceModelCorrelationsSection section)
            : base(section) {
        }

        public override string Title => "Pearson correlations chart of the assessment group membership models.";

        public override string ChartId {
            get {
                var pictureId = "290052B1-AFE8-43C4-8ADC-768AA0FF96E1";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
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

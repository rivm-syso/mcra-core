using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class QsarMembershipModelPearsonCorrelationsChartCreator : QsarMembershipModelCorrelationsChartCreatorBase {

        public QsarMembershipModelPearsonCorrelationsChartCreator(QsarMembershipModelCorrelationsSection section)
            : base(section) {
        }

        public override string Title => "QSAR model correlations (Pearson)";

        public override string ChartId {
            get {
                var pictureId = "CB9A9C87-D5BF-4BA1-B25B-7880876693DD";
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

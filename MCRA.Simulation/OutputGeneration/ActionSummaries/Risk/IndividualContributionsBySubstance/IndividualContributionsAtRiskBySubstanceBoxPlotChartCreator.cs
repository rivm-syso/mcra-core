using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualContributionsAtRiskBySubstanceBoxPlotChartCreator : IndividualContributionsUpperBySubstanceBoxPlotChartCreator {

        public IndividualContributionsAtRiskBySubstanceBoxPlotChartCreator(
            ContributionsForIndividualsUpperSection section,
            bool showOutliers
        ) : base (section, showOutliers) {
        }

        public override string ChartId {
            get {
                var pictureId = "303FB243-7383-4391-A37A-8FCA83758AF5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
    }
}

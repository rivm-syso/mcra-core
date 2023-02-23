using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MtaDistributionByCategoryChartCreator : MtaDistributionByCategoryChartCreatorBase {

        private readonly UsualIntakeDistributionPerCategorySection _section;
        private readonly string _intakeUnit;

        public MtaDistributionByCategoryChartCreator(
            UsualIntakeDistributionPerCategorySection section,
            string intakeUnit,
            bool showContributions,
            int width = 500,
            int height = 350
        ) {
            ShowContributions = showContributions;
            Width = width;
            Height = height;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = ShowContributions
                    ? "1bddd03f-b3a7-47fb-a067-0d5ae1944339"
                    : "b02aaa34-f077-47af-8a9b-add9ad44fcf5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var positiveGroupIndividualExposures = _section.IndividualExposuresByCategory
                .Where(ia => ia.TotalExposure > 0)
                .ToList();
            var fractionPositives = positiveGroupIndividualExposures.Sum(r => r.SamplingWeight) / _section.IndividualExposuresByCategory.Sum(r => r.SamplingWeight);
            var plotModel = create(positiveGroupIndividualExposures, _section.Categories, _intakeUnit);
            plotModel.Title = ShowContributions
                ? $"Contribution per category to exposure distribution, {fractionPositives * 100:F1}% positives"
                : $"Total usual exposure distribution modelled as OIM ({fractionPositives * 100:F1} % positives)";
            plotModel.ClipTitle = false;
            plotModel.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinView;
            return plotModel;
        }

    }
}
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class MtaDistributionByFoodAsMeasuredChartCreator : MtaDistributionByCategoryChartCreatorBase {

        protected readonly UsualIntakeDistributionPerFoodAsMeasuredSection _section;
        protected readonly string _intakeUnit;

        public MtaDistributionByFoodAsMeasuredChartCreator(
            UsualIntakeDistributionPerFoodAsMeasuredSection section,
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
                    ? "0b0067e6-0754-45a7-a8dd-80e5dd9496bb"
                    : "6ac25cc0-3e9e-4d34-ba8d-165a33af89ff";
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
                ? $"Contribution per food to total exposure distribution, ({fractionPositives * 100:F1}% positives)"
                : $"Total usual exposure distribution modelled as OIM, {fractionPositives * 100:F1}% positives";
            return plotModel;
        }
    }
}

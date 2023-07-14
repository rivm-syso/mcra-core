using MCRA.Utils.ExtensionMethods;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class MtaOthersDistributionByFoodAsMeasuredChartCreator : MtaDistributionByFoodAsMeasuredChartCreator {

        private readonly ICollection<IntakeModelPerCategoryDto> _groupings;

        public MtaOthersDistributionByFoodAsMeasuredChartCreator(
            UsualIntakeDistributionPerFoodAsMeasuredSection section,
            ICollection<IntakeModelPerCategoryDto> groupings,
            string intakeUnit,
            bool showContributions,
            int width = 500,
            int height = 350
        ) : base (section, intakeUnit, showContributions, width, height) {
            _groupings = groupings;
        }

        public override string ChartId {
            get {
                var pictureId = ShowContributions
                    ? "A66BE227-FDD4-49AF-83AA-7794ACD3C37A"
                    : "109048AE-CCAC-4414-84FC-C866DACA0DF9";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var plotModel = createOthersByCategory(_section, _groupings, _intakeUnit);
            return plotModel;
        }

        private PlotModel createOthersByCategory(
            UsualIntakeDistributionPerCategorySectionBase section,
            ICollection<IntakeModelPerCategoryDto> groupings,
            string intakeUnit
        ) {
            var tabuSet = groupings
                .SelectMany(r => r.FoodsAsMeasured)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var groupIndividualExposures = section.IndividualExposuresByCategory
                .Select(i => new CategorizedIndividualExposure() {
                    SimulatedIndividualId = i.SimulatedIndividualId,
                    SamplingWeight = i.SamplingWeight,
                    CategoryExposures = i.CategoryExposures.Where(r => !tabuSet.Contains(r.IdCategory)).ToList()
                });
            var positiveGroupIndividualExposures = groupIndividualExposures
                .Where(r => r.TotalExposure > 0)
                .ToList();
            var fractionPositives = positiveGroupIndividualExposures.Sum(r => r.SamplingWeight) / groupIndividualExposures.Sum(r => r.SamplingWeight);
            var plotModel = create(positiveGroupIndividualExposures, _section.Categories, intakeUnit);
            plotModel.Title = ShowContributions
                ? $"Others distribution contributions by category ({fractionPositives * 100:F1}% positives)"
                : $"Others distributions by category ({fractionPositives * 100:F1}% positives)";
            return plotModel;
        }
    }
}

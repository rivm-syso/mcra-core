using MCRA.Utils.ExtensionMethods;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class MtaGroupingDistributionByFoodAsMeasuredChartCreator : MtaDistributionByFoodAsMeasuredChartCreator {

        private readonly IntakeModelPerCategoryDto _grouping;
        private readonly string _groupName;

        public MtaGroupingDistributionByFoodAsMeasuredChartCreator(
            UsualIntakeDistributionPerFoodAsMeasuredSection section,
            IntakeModelPerCategoryDto grouping,
            string groupName,
            string intakeUnit,
            bool showContributions,
            int width = 500,
            int height = 350
        ) : base (section, intakeUnit, showContributions, width, height) {
            _grouping = grouping;
            _groupName = groupName;
        }

        public override string ChartId {
            get {
                var pictureId = ShowContributions
                    ? "C7C5D40C-C20F-46FD-A5B8-FD01FFE27D23"
                    : "11B322FA-F14E-444A-9521-210D5F1552B0";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var plotModel = createOthersByCategory(_section, _grouping, _intakeUnit);
            return plotModel;
        }

        private PlotModel createOthersByCategory(
            UsualIntakeDistributionPerCategorySectionBase section,
            IntakeModelPerCategoryDto grouping,
            string intakeUnit
        ) {
            var tabuSet = grouping.FoodsAsMeasured.ToHashSet();

            var groupIndividualExposures = section.IndividualExposuresByCategory
                .Select(i => new CategorizedIndividualExposure() {
                    SimulatedIndividualId = i.SimulatedIndividualId,
                    SamplingWeight = i.SamplingWeight,
                    CategoryExposures = i.CategoryExposures.Where(r => tabuSet.Contains(r.IdCategory)).ToList()
                })
                .ToList();
            var positiveGroupIndividualExposures = groupIndividualExposures
                .Where(r => r.TotalExposure > 0)
                .ToList();
            var fractionPositives = positiveGroupIndividualExposures.Sum(r => r.SamplingWeight) / groupIndividualExposures.Sum(r => r.SamplingWeight);
            var plotModel = create(positiveGroupIndividualExposures, _section.Categories, intakeUnit);
            plotModel.Title = ShowContributions
                ? $"{_groupName} distribution contributions by category ({fractionPositives * 100:F1}% positives)"
                : $"{_groupName} distributions by category ({fractionPositives * 100:F1}% positives)";
            return plotModel;
        }
    }
}

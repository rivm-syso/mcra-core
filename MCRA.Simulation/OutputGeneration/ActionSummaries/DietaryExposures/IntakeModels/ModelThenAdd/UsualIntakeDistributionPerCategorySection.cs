using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UsualIntakeDistributionPerCategorySection : UsualIntakeDistributionPerCategorySectionBase {

        public List<UsualIntakeDistributionPerCategoryModelSection> UsualIntakeDistributionPerCategoryModelSections { get; set; }

        public void Summarize(CompositeIntakeModel compositeIntakeModel) {
            var detailedObservedIndividualMeansPerCategory = computeCategorizedObservedIndividualMeans(compositeIntakeModel);
            Categories = compositeIntakeModel.PartialModels
                .Select(r => new Category() {
                    Id = $"Model{r.ModelIndex}",
                    Name = $"Model {r.ModelIndex} - {r.IntakeModel.IntakeModelType.GetDisplayName()}"
                })
                .ToList();
            IndividualExposuresByCategory = detailedObservedIndividualMeansPerCategory
                .Where(ia => ia.TotalExposure > 0)
                .ToList();
            UsualIntakeDistributionPerCategoryModelSections = [];
            foreach (var model in compositeIntakeModel.PartialModels) {
                var distributionPerModelSection = new UsualIntakeDistributionPerCategoryModelSection();
                var intakes = model.IndividualIntakes.Select(ui => ui.DietaryIntakePerMassUnit).ToList();
                var weights = model.IndividualIntakes.Select(ui => ui.SimulatedIndividual.SamplingWeight).ToList();
                distributionPerModelSection.Summarize(intakes, weights);
                distributionPerModelSection.FoodNames = string.Join(", ", model.FoodsAsMeasured.Select(c => c.Name.ToLowerInvariant()));
                UsualIntakeDistributionPerCategoryModelSections.Add(distributionPerModelSection);
            }
        }

        public void SummarizeUncertainty(
            List<UsualIntakeDistributionPerCategoryModelSection> usualIntakeDistributionPerCategoryModelSections,
            CompositeIntakeModel compositeIntakeModel,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var counter = 0;
            foreach (var model in compositeIntakeModel.PartialModels) {
                var distributionPerModelSection = usualIntakeDistributionPerCategoryModelSections[counter];
                var intakes = model.IndividualIntakes.Select(ui => ui.DietaryIntakePerMassUnit).ToList();
                var weights = model.IndividualIntakes.Select(ui => ui.SimulatedIndividual.SamplingWeight).ToList();
                distributionPerModelSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
                counter++;
            }
        }

        /// <summary>
        /// Computes the categorized observed individual means of the composite intake model
        /// by combining all observed individual means of the partial models into categorized
        /// individual intakes.
        /// </summary>
        /// <param name="compositeIntakeModel"></param>
        /// <returns></returns>
        private static List<CategorizedIndividualExposure> computeCategorizedObservedIndividualMeans(CompositeIntakeModel compositeIntakeModel) {
            return compositeIntakeModel.PartialModels
                 .SelectMany(t => t.IndividualIntakes, (t, i) => (
                     Model: t,
                     IndividualIntakes: i
                 ))
                 .GroupBy(gr => gr.IndividualIntakes.SimulatedIndividual.Id)
                 .Select(g => new CategorizedIndividualExposure() {
                     SimulatedIndividualId = g.Key,
                     SamplingWeight = g.First().IndividualIntakes.SimulatedIndividual.SamplingWeight,
                     CategoryExposures = g
                         .Select(r => new CategoryExposure() {
                             IdCategory = $"{r.Model.ModelIndex}: {r.Model.IntakeModel.IntakeModelType.GetDisplayAttribute().ShortName} (n={r.Model.FoodsAsMeasured.Count})",
                             Exposure = r.IndividualIntakes.DietaryIntakePerMassUnit
                         })
                        .Where(r => r.Exposure > 0)
                         .ToList()
                 })
                 .ToList();
        }
    }
}

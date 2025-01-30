using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorModelCollectionBuilder {

        public ICollection<ProcessingFactorModel> Create(
            ICollection<ProcessingFactor> processingFactors,
            ICollection<Compound> substances,
            bool createDistributionModels,
            bool allowHigherThanOne
        ) {
            var result = new Dictionary<(Food, Compound, ProcessingType), ProcessingFactorModel>();
            var substanceSpecificRecords = processingFactors
                .Where(r => r.Compound != null)
                .ToList();

            foreach (var processingFactor in substanceSpecificRecords) {
                if (!result.TryGetValue((processingFactor.FoodUnprocessed, processingFactor.Compound, processingFactor.ProcessingType), out var model)) {
                    model = createProcessingFactorModel(
                        processingFactor.FoodUnprocessed,
                        processingFactor.Compound,
                        processingFactor.ProcessingType,
                        processingFactor,
                        createDistributionModels,
                        allowHigherThanOne
                    );
                    result.Add((processingFactor.FoodUnprocessed, processingFactor.Compound, processingFactor.ProcessingType), model);
                }
            }

            var substanceGenericRecords = processingFactors
                .Where(r => r.Compound == null)
                .ToList();
            foreach (var foodProcessingFactor in substanceGenericRecords) {
                foreach (var substance in substances) {
                    if (!result.TryGetValue((foodProcessingFactor.FoodUnprocessed, substance, foodProcessingFactor.ProcessingType), out var model)) {
                        model = createProcessingFactorModel(
                            foodProcessingFactor.FoodUnprocessed,
                            substance,
                            foodProcessingFactor.ProcessingType,
                            foodProcessingFactor,
                            createDistributionModels,
                            allowHigherThanOne
                        );
                        result.Add((foodProcessingFactor.FoodUnprocessed, substance, foodProcessingFactor.ProcessingType), model);
                    }
                }
            }

            return result.Values;
        }

        public void Resample(
            IRandom random,
            ICollection<ProcessingFactorModel> processingFactorProvider
        ) {
            var modelsOrdered = processingFactorProvider
                .OrderBy(c => c.Food.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.Substance.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.ProcessingType.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var model in modelsOrdered) {
                model.Resample(random);
            }
        }

        public void ResetNominal(
            ICollection<ProcessingFactorModel> processingFactorModels
        ) {
            foreach (var model in processingFactorModels) {
                model.ResetNominal();
            }
        }

        private ProcessingFactorModel createProcessingFactorModel(
            Food food,
            Compound substance,
            ProcessingType processingType,
            ProcessingFactor pf,
            bool createDistributionModels,
            bool allowHigherThanOne
        ) {
            ProcessingFactorModel model = null;

            if (createDistributionModels) {
                if (allowHigherThanOne) {
                    if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogisticNormal) {
                        model = new PFFixedAllowHigherModel(pf);
                    } else if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogNormal) {
                        if (pf.Upper.Value < 1) {
                            model = new PFFixedAllowHigherModel(pf);
                        } else {
                            model = new PFLogNormalAllowHigherModel(pf);
                        }
                    }
                } else {
                    if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogisticNormal) {
                        model = new PFLogisticModel(pf);
                    } else if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogNormal) {
                        model = new PFLogNormalModel(pf);
                    }
                }
            } else {
                model = allowHigherThanOne
                    ? new PFFixedAllowHigherModel(pf)
                    : new PFFixedModel(pf);
            }

            model.CalculateParameters();
            return model;
        }
    }
}

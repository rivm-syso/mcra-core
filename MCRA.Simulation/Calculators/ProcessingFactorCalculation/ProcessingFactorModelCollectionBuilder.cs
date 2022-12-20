using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public sealed class ProcessingFactorModelCollectionBuilder {
        private readonly IProcessingFactorModelCollectionBuilderSettings _settings;
        public ProcessingFactorModelCollectionBuilder(IProcessingFactorModelCollectionBuilderSettings settings) {
            _settings = settings;
        }

        public ProcessingFactorModelCollection Create(
            
            ICollection<ProcessingFactor> processingFactors,
            ICollection<Compound> substances
        ) {
            var result = new ThreeKeyDictionary<Food, Compound, ProcessingType, ProcessingFactorModel>();
            if (processingFactors != null && _settings.IsProcessing) {
                var substanceSpecificRecords = processingFactors
                    .Where(r => r.Compound != null)
                    .ToList();
                foreach (var processingFactor in substanceSpecificRecords) {
                    if (!result.TryGetValue(processingFactor.FoodUnprocessed, processingFactor.Compound, processingFactor.ProcessingType, out var model)) {
                        model = createProcessingFactorModel(
                            processingFactor.FoodUnprocessed,
                            processingFactor.Compound,
                            processingFactor.ProcessingType,
                            processingFactor,
                            _settings
                        );
                        result.Add(processingFactor.FoodUnprocessed, processingFactor.Compound, processingFactor.ProcessingType, model);
                    }
                }
                var substanceGenericRecords = processingFactors
                    .Where(r => r.Compound == null)
                    .ToList();
                foreach (var foodProcessingFactor in substanceGenericRecords) {
                    foreach (var substance in substances) {
                        if (!result.TryGetValue(foodProcessingFactor.FoodUnprocessed, substance, foodProcessingFactor.ProcessingType, out var model)) {
                            model = createProcessingFactorModel(
                                foodProcessingFactor.FoodUnprocessed,
                                substance,
                                foodProcessingFactor.ProcessingType,
                                foodProcessingFactor,
                                _settings
                            );
                            result.Add(foodProcessingFactor.FoodUnprocessed, substance, foodProcessingFactor.ProcessingType, model);
                        }
                    }
                }
            }
            return new ProcessingFactorModelCollection(result);
        }

        public void Resample(IRandom random, ProcessingFactorModelCollection processingFactorModels) {
            if (processingFactorModels != null) {
                var _modelsOrdered = processingFactorModels.Values
                    .OrderBy(c => c.Food.Code, System.StringComparer.OrdinalIgnoreCase)
                    .ThenBy(c => c.Substance.Code, System.StringComparer.OrdinalIgnoreCase)
                    .ThenBy(c => c.ProcessingType.Code, System.StringComparer.OrdinalIgnoreCase)
                    .ToList();
                foreach (var model in processingFactorModels.Values) {
                    model.Resample(random);
                }
            }
        }

        public void ResetNominal(ProcessingFactorModelCollection processingFactorModels) {
            foreach (var model in processingFactorModels.Values) {
                model.ResetNominal();
            }
        }

        private ProcessingFactorModel createProcessingFactorModel(
            Food food,
            Compound substance,
            ProcessingType processingType,
            ProcessingFactor pf,
            IProcessingFactorModelCollectionBuilderSettings settings
        ) {
            ProcessingFactorModel model = null;

            if (settings.IsProcessing) {
                if (settings.IsDistribution) {
                    if (settings.AllowHigherThanOne) {
                        if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogisticNormal) {
                            model = new PFFixedAllowHigherModel();
                        } else if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogNormal) {
                            if (pf.Upper.Value < 1) {
                                model = new PFFixedAllowHigherModel();
                            } else {
                                model = new PFLogNormalAllowHigherModel();
                            }
                        }
                    } else {
                        if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogisticNormal) {
                            model = new PFLogisticModel();
                        } else if (pf.ProcessingType.DistributionType == ProcessingDistributionType.LogNormal) {
                            model = new PFLogNormalModel();
                        }
                    }
                } else {
                    model = settings.AllowHigherThanOne
                          ? new PFFixedAllowHigherModel()
                          : (ProcessingFactorModel)new PFFixedModel();
                }
            }

            model.ProcessingType = processingType;
            model.Food = food;
            model.Substance = substance;
            model.CalculateParameters(pf);
            return model;
        }
    }
}

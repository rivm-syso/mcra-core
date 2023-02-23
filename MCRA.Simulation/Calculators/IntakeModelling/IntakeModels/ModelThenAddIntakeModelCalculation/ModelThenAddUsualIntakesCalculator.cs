using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ModelThenAddIntakeModelCalculation {
    public class ModelThenAddUsualIntakesCalculator {

        private CancellationToken? _cancellationToken;

        public ModelThenAddUsualIntakesCalculator(
            CancellationToken? cancellationToken
        ) {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Calculates the model then add individual usual intakes.
        /// </summary>
        /// <param name="compositeIntakeModel"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="numberOfMonteCarloIterations"></param>
        /// <param name="frequencyCovariateModelType"></param>
        /// <param name="amountCovariateModelType"></param>
        /// <param name="randomSeedModelBasedIntakesGeneration"></param>
        /// <param name="randomSeedModelAssisstedIntakesGeneration"></param>
        /// <returns></returns>
        public ModelThenAddUsualIntakeResults CalculateUsualIntakes(
            CompositeIntakeModel compositeIntakeModel,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            int numberOfMonteCarloIterations,
            CovariateModelType frequencyCovariateModelType,
            CovariateModelType amountCovariateModelType,
            int randomSeedModelBasedIntakesGeneration,
            int randomSeedModelAssisstedIntakesGeneration
        ) {
            foreach (var intakeModel in compositeIntakeModel.PartialModels) {
                if (!(intakeModel.IntakeModel is OIMModel)) {
                    intakeModel.MTAModelAssistedIntakes = intakeModel.IntakeModel
                        .GetIndividualIntakes(randomSeedModelAssisstedIntakesGeneration);
                } else {
                    intakeModel.MTAModelAssistedIntakes = intakeModel.IndividualIntakes
                        .Select(c => new ModelAssistedIntake() {
                            Individual = c.Individual,
                            IndividualSamplingWeight = c.IndividualSamplingWeight,
                            UsualIntake = c.DietaryIntakePerMassUnit,
                            SimulatedIndividualId = c.SimulatedIndividualId,
                        })
                        .ToList();
                }
            }
            var mtaModelAssistedIntakesGrouped = compositeIntakeModel.PartialModels
                .SelectMany(c => c.MTAModelAssistedIntakes)
                .GroupBy(gr => gr.SimulatedIndividualId)
                .ToList();

            var dayIntakesGroupedByIndividual = dietaryIndividualDayIntakes
                .GroupBy(idi => idi.SimulatedIndividualId)
                .ToList();

            var modelBasedIntakesGenerator = Simulation.IsBackwardCompatibilityMode
                ? new McraRandomGenerator(randomSeedModelBasedIntakesGeneration, true)
                : new McraRandomGenerator(randomSeedModelBasedIntakesGeneration);

            var mtaModelBasedIntakes = getModelThanAddUsualIntake(
                dietaryIndividualDayIntakes,
                compositeIntakeModel,
                numberOfMonteCarloIterations,
                frequencyCovariateModelType,
                amountCovariateModelType,
                modelBasedIntakesGenerator
           );

            return new ModelThenAddUsualIntakeResults() {
                DietaryModelAssistedIntakes = mtaModelAssistedIntakesGrouped
                    .Select(c => new DietaryIndividualIntake() {
                        DietaryIntakePerMassUnit = c.Sum(a => a.UsualIntake),
                        Individual = c.Last().Individual,
                        IndividualSamplingWeight = c.First().IndividualSamplingWeight,
                        SimulatedIndividualId = c.Last().SimulatedIndividualId,
                    })
                    .ToList(),
                DietaryModelBasedIntakeResults = mtaModelBasedIntakes,
            };
        }

        /// <summary>
        /// Get model than add usual exposure from exposure models.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="compositeIntakeModel"></param>
        /// <param name="numberOfMonteCarloIterations"></param>
        /// <param name="frequencyCovariateModelType"></param>
        /// <param name="amountCovariateModelType"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        private List<ModelBasedIntakeResult> getModelThanAddUsualIntake(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            CompositeIntakeModel compositeIntakeModel,
            int numberOfMonteCarloIterations,
            CovariateModelType frequencyCovariateModelType,
            CovariateModelType amountCovariateModelType,
            IRandom generator
        ) {
            var observedIndividualMeansRemainingModel = compositeIntakeModel.PartialModels
                .Where(c => c.IntakeModel is OIMModel)
                .SelectMany(c => c.MTAModelAssistedIntakes)
                .GroupBy(gr => gr.SimulatedIndividualId)
                .Select(ui => (
                    SimulatedIndividualId: ui.Key,
                    ObservedIndividualMean: ui.Sum(c => c.UsualIntake)
                ))
                .ToLookup(item => item.SimulatedIndividualId, item => item);

            //check if one or more uncorrelated models are present
            var isCovariateModelling = compositeIntakeModel.PartialModels
                .Any(c => c.IntakeModel is BBNModel || c.IntakeModel is LNN0Model);
            var useCofactor = false;
            var useCovariable = false;
            if (isCovariateModelling) {
                (useCofactor, useCovariable) = getCovariateModelType(frequencyCovariateModelType, amountCovariateModelType);
            }

            var mtaCovariateGroups = dietaryIndividualDayIntakes
                .GroupBy(gr => gr.SimulatedIndividualId)
                .Select(c => new CovariateGroupMTA {
                    CovariateGroup = new CovariateGroup {
                        Cofactor = useCofactor ? c.First().Individual.Cofactor : null,
                        Covariable = useCovariable ? c.First().Individual.Covariable : double.NaN,
                        GroupSamplingWeight = c.First().IndividualSamplingWeight,
                    },
                    ObservedIndividualMeanRemainingCategory = observedIndividualMeansRemainingModel[c.Key].Sum(a => a.ObservedIndividualMean),
                    ModelBasedIntakeResults = new List<ModelBasedIntakeResult>(),
                    SimulatedIndividualId = c.Key,
                    Individual = c.First().Individual,
                })
                .ToList();

            foreach (var partialModel in compositeIntakeModel.PartialModels) {
                calculateMarginalIntakes(
                    mtaCovariateGroups,
                    partialModel.IntakeModel,
                    generator.Next(),
                    numberOfMonteCarloIterations
                );
            }
            var marginalIntakeResults = mtaCovariateGroups
                .Select(c => new ModelBasedIntakeResult {
                    CovariateGroup = c.CovariateGroup,
                    ModelBasedIntakes = c.ModelBasedIntakes
                })
                .ToList();
            return marginalIntakeResults;
        }

        /// <summary>
        /// Calculates marginal usual exposures
        /// </summary>
        /// <param name="CovariateAmountGroups"></param>
        /// <param name="intakeModel"></param>
        /// <param name="seed"></param>
        /// <param name="numberOfMonteCarloIterations"></param>
        /// <param name="progressState"></param>
        private void calculateMarginalIntakes(
            List<CovariateGroupMTA> CovariateAmountGroups,
            IntakeModel intakeModel,
            int seed,
            int numberOfMonteCarloIterations
        ) {
            if (!(intakeModel is OIMModel)) {
                var n = numberOfMonteCarloIterations / CovariateAmountGroups.Sum(c => c.CovariateGroup.GroupSamplingWeight);
                var numberOfIterationsPerIndividual = n > 0 ? n : 1;
                var cancelToken = _cancellationToken ?? new System.Threading.CancellationToken();

                Parallel.ForEach(
                    CovariateAmountGroups,
                    new ParallelOptions() { MaxDegreeOfParallelism = 1000, CancellationToken = cancelToken },
                    (covariateAmountGroup) => {
                        CovariateGroup acg;
                        var usualIntakes = new List<double>();
                        var nSimulations = BMath.Ceiling(covariateAmountGroup.CovariateGroup.GroupSamplingWeight * numberOfIterationsPerIndividual);
                        var random = Simulation.IsBackwardCompatibilityMode
                            ? new McraRandomGenerator(covariateAmountGroup.CovariateGroup.GetHashCode() + seed, true)
                            : new McraRandomGenerator(RandomUtils.CreateSeed(seed, covariateAmountGroup.CovariateGroup.GetHashCode()));

                        if (intakeModel is BBNModel) {
                            var bbnModel = intakeModel as BBNModel;
                            var frequencyPredictions = bbnModel.FrequencyModel.ConditionalPredictions;
                            (var frequencyDistribution, var fcg) = bbnModel.FrequencyModel.GetDistribution(frequencyPredictions, covariateAmountGroup.CovariateGroup);
                            var amountsPredictions = bbnModel.AmountModel.ConditionalPredictions;
                            var amountsDistribution = bbnModel.AmountModel.GetDistribution(amountsPredictions, covariateAmountGroup.CovariateGroup, out acg);
                            for (int i = 0; i < nSimulations; i++) {
                                usualIntakes.Add(frequencyDistribution.Draw(random) * amountsDistribution.Draw(random));
                            }
                        } else {
                            var lnn0Model = intakeModel as LNN0Model;
                            var frequencyPredictions = lnn0Model.FrequencyModel.ConditionalPredictions;
                            (var frequencyDistribution, var fcg) = lnn0Model.FrequencyModel.GetDistribution(frequencyPredictions, covariateAmountGroup.CovariateGroup);
                            var amountsPredictions = lnn0Model.AmountModel.ConditionalPredictions;
                            var amountsDistribution = lnn0Model.AmountModel.GetDistribution(amountsPredictions, covariateAmountGroup.CovariateGroup, out acg);
                            for (int i = 0; i < nSimulations; i++) {
                                usualIntakes.Add(frequencyDistribution.Draw(random) * amountsDistribution.Draw(random));
                            }
                        }
                        var marginalIntakeResult = new ModelBasedIntakeResult() {
                            CovariateGroup = covariateAmountGroup.CovariateGroup,
                            ModelBasedIntakes = usualIntakes,
                        };

                        covariateAmountGroup.ModelBasedIntakeResults.Add(marginalIntakeResult);
                    }
                );
            }
        }

        /// <summary>
        /// Get cofactor and covariable setting
        /// </summary>
        /// <param name="frequencyCovariateModelType"></param>
        /// <param name="amountCovariateModelType"></param>
        /// <returns></returns>
        private (bool UseCovariable, bool UsCofactor) getCovariateModelType(
            CovariateModelType frequencyCovariateModelType,
            CovariateModelType amountCovariateModelType
        ) {
            var frequencyModelType = CovariateModelType.Constant;
            switch (frequencyCovariateModelType) {
                case CovariateModelType.Constant:
                    frequencyModelType = CovariateModelType.Constant;
                    break;
                case CovariateModelType.Covariable:
                    frequencyModelType = CovariateModelType.Covariable;
                    break;
                case CovariateModelType.Cofactor:
                    frequencyModelType = CovariateModelType.Cofactor;
                    break;
                case CovariateModelType.CovariableCofactor:
                    frequencyModelType = CovariateModelType.CovariableCofactor;
                    break;
                case CovariateModelType.CovariableCofactorInteraction:
                    frequencyModelType = CovariateModelType.CovariableCofactorInteraction;
                    break;
            }

            var amountModelType = CovariateModelType.Constant;
            switch (amountCovariateModelType) {
                case CovariateModelType.Constant:
                    amountModelType = CovariateModelType.Constant;
                    break;
                case CovariateModelType.Covariable:
                    amountModelType = CovariateModelType.Covariable;
                    break;
                case CovariateModelType.Cofactor:
                    amountModelType = CovariateModelType.Cofactor;
                    break;
                case CovariateModelType.CovariableCofactor:
                    amountModelType = CovariateModelType.CovariableCofactor;
                    break;
                case CovariateModelType.CovariableCofactorInteraction:
                    amountModelType = CovariateModelType.CovariableCofactorInteraction;
                    break;
            }

            if (frequencyModelType == CovariateModelType.Constant && amountModelType == CovariateModelType.Constant) {
                //Constant 
                return (false, false);
            } else if ((frequencyModelType == CovariateModelType.Cofactor && amountModelType == CovariateModelType.Constant) ||
                  (frequencyModelType == CovariateModelType.Cofactor && amountModelType == CovariateModelType.Cofactor) ||
                  (frequencyModelType == CovariateModelType.Constant && amountModelType == CovariateModelType.Cofactor)
                  ) {
                //Cofactor
                return (true, false);
            } else if ((frequencyModelType == CovariateModelType.Covariable && amountModelType == CovariateModelType.Constant) ||
                  (frequencyModelType == CovariateModelType.Covariable && amountModelType == CovariateModelType.Covariable) ||
                  (frequencyModelType == CovariateModelType.Constant && amountModelType == CovariateModelType.Covariable)
                  ) {
                //Covariable
                return (false, true);
            } else {
                //CovariableAndCofactor
                return (true, true);
            }
        }
    }
}

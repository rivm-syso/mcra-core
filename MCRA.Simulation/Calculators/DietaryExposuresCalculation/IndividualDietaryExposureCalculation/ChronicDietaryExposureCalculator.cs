using System.Collections.Concurrent;
using log4net;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.TdsReductionFactorsCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {
    public sealed class ChronicDietaryExposureCalculator : DietaryExposureCalculatorBase {

        private static readonly ILog log = LogManager.GetLogger(typeof(ChronicDietaryExposureCalculator));
        private static readonly object _marketShareLock = new();
        private readonly IResidueGenerator _residueGenerator;
        private readonly bool _isTotalDietStudy;
        private readonly bool _isScenarioAnalysis;
        private readonly IDictionary<(Food, Compound), double> _tdsReductionFactors;
        private readonly ConcurrentDictionary<(Individual, Food, Food), double> _marketSharesDictionary = new();

        public ChronicDietaryExposureCalculator(
            ICollection<Compound> activeSubstances,
            IDictionary<(Food, Compound), double> tdsReductionFactors,
            IDictionary<(Individual, string), List<ConsumptionsByModelledFood>> consumptionsByFoodsAsMeasured,
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels,
            IIndividualDayIntakePruner individualDayIntakePruner,
            ProcessingFactorModelCollection processingFactorModels,
            IResidueGenerator residueGenerator,
            bool isTotalDietStudy,
            bool isScenarioAnalysis
        ) : base(
              consumptionsByFoodsAsMeasured,
              processingFactorModels,
              activeSubstances,
              individualDayIntakePruner
        ) {
            _consumptionsByFoodsAsMeasured = consumptionsByFoodsAsMeasured;
            _tdsReductionFactors = tdsReductionFactors;
            _isTotalDietStudy = isTotalDietStudy;
            _isScenarioAnalysis = isScenarioAnalysis;
            _residueGenerator = residueGenerator;
        }

        /// <summary>
        /// Calculate chronic dietary daily intakes
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public override List<DietaryIndividualDayIntake> CalculateDietaryIntakes(
            List<SimulatedIndividualDay> simulatedIndividualDays,
            ProgressState progressState,
            int randomSeed
        ) {
            void logProgress(string message, double percentCompleted) {
                log.Info(message);
                progressState.CurrentActivity = message;
                progressState.Progress = percentCompleted;
            }

            logProgress("Calculating individual-day exposures", 50);
            int partitionSize = 100;
            var isScenario = _isTotalDietStudy && _isScenarioAnalysis;

            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();
            var result = simulatedIndividualDays
                .Partition(partitionSize)
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(partition => {
                    var intakes = new List<DietaryIndividualDayIntake>(partitionSize);
                    foreach (var individualDay in partition) {
                        var seed = randomSeed + individualDay.SimulatedIndividualDayId;
                        var dietaryIndividualDayIntake = calculateIndividualDayIntake(isScenario, individualDay, seed);
                        var prunedIndividualDayIntake = (_individualDayIntakePruner != null)
                            ? _individualDayIntakePruner?.Prune(dietaryIndividualDayIntake)
                            : dietaryIndividualDayIntake;
                        intakes.Add(prunedIndividualDayIntake);
                    }
                    return intakes;
                });

            logProgress("Finished calculating dietary individual-day exposures", 100);
            return result.SelectMany(c => c).OrderBy(c => c.SimulatedIndividualDayId).ToList();
        }


        /// <summary>
        /// Override: compute the exposures per substance.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <returns></returns>
        public override Dictionary<Compound, List<ExposureRecord>> ComputeExposurePerCompoundRecords(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            var numberOfIntakes = dietaryIndividualDayIntakes.Count;

            var emptyDays = dietaryIndividualDayIntakes
                .GroupBy(gr => gr.SimulatedIndividualId)
                .Where(gr => !gr.Any(r => r.IntakesPerFood.Any(ipf => ipf.IntakesPerCompound.Any(ipc => ipc.Exposure > 0))))
                .Select(c => (
                    SimulatedIndividualId: c.Key,
                    BodyWeight: c.First().Individual.BodyWeight,
                    SamplingWeight: c.First().IndividualSamplingWeight
                ))
                .ToList();

            var exposurePerCompoundRecords = dietaryIndividualDayIntakes
                    .SelectMany(idi => {
                        var dietaryIntakesPerCompound = idi.IntakesPerFood
                                .SelectMany(ipf => ipf.IntakesPerCompound)
                                .GroupBy(gr => gr.Compound)
                                .Select(g => (
                                    Compound: g.Key,
                                    Individual: idi.Individual,
                                    IndividualSamplingWeight: idi.IndividualSamplingWeight,
                                    SimulatedIndividualId: idi.SimulatedIndividualId,
                                    Intake: g.Sum(ipc => ipc.Exposure)
                                ))
                                .ToList();
                        return dietaryIntakesPerCompound;
                    })
                    .GroupBy(gr => (gr.Compound, gr.SimulatedIndividualId))
                    .Select(c => (
                        Compound: c.Key.Compound,
                        SimulatedIndividualId: c.First().SimulatedIndividualId,
                        Intake: c.Sum(ipc => ipc.Intake) / c.Count(),
                        SamplingWeight: c.First().IndividualSamplingWeight,
                        BodyWeight: c.First().Individual.BodyWeight
                    ))
                    .GroupBy(a => a.Compound)
                    .Select(g => {
                        return (
                            Compound: g.Key,
                            ExposureRecords: g.Select(s => new ExposureRecord() {
                                IndividualDayId = s.SimulatedIndividualId,
                                Exposure = s.Intake,
                                BodyWeight = s.BodyWeight,
                                SamplingWeight = s.SamplingWeight
                            }).ToList()
                        );
                    })
                    .OrderBy(r => r.Compound.Code, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(c => c.Compound, c => c.ExposureRecords);

            ///Some individual days have no consumption, socalled empty days.
            foreach (var day in emptyDays) {
                foreach (var item in exposurePerCompoundRecords) {
                    item.Value.Add(new ExposureRecord {
                        IndividualDayId = day.SimulatedIndividualId,
                        Exposure = 0,
                        SamplingWeight = day.SamplingWeight,
                        BodyWeight = day.BodyWeight,
                    });
                }
            }
            return exposurePerCompoundRecords;
        }

        /// <summary>
        /// Simulates one individual day. Generates concentrations for the residues on the consumptions.
        /// </summary>
        /// <param name="isTdsReductionToLimitScenario"></param>
        /// <param name="sid"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private DietaryIndividualDayIntake calculateIndividualDayIntake(
            bool isTdsReductionToLimitScenario,
            SimulatedIndividualDay sid,
            int seed
        ) {
            IRandom marketSharesRandomGenerator;
            IRandom processingFactorsRandomGenerator;
            if (Simulation.IsBackwardCompatibilityMode) {
                // TODO: replace on switch random number generator
                var dietaryRandomGenerator = new McraRandomGenerator(seed, true);
                processingFactorsRandomGenerator = dietaryRandomGenerator;
                marketSharesRandomGenerator = dietaryRandomGenerator;
            } else {
                var random = new McraRandomGenerator(seed);
                marketSharesRandomGenerator = new McraRandomGenerator(random.Next(), true);
                processingFactorsRandomGenerator = new McraRandomGenerator(random.Next(), true);
            }

            if (!_consumptionsByFoodsAsMeasured.TryGetValue((sid.Individual, sid.Day), out var consumptions)) {
                consumptions = new List<ConsumptionsByModelledFood>();
            }

            //consumption may contain brands (or marketshares) so select/draw the relevant foods, brands and shares
            var marketShares = getMarketShares(marketSharesRandomGenerator, consumptions);
            var intakesPerFood = new List<IIntakePerFood>(consumptions.Count);
            foreach (var consumption in consumptions) {
                //For substance dependent food conversion paths, select the relevant substances based on conversion results, see issue #1090
                var concentrations = _isSubstanceDependent
                     ? _residueGenerator.GenerateResidues(consumption.FoodAsMeasured, _selectedSubstances.Intersect(consumption.ConversionResultsPerCompound.Keys).ToList(), null)
                     : _residueGenerator.GenerateResidues(consumption.FoodAsMeasured, _selectedSubstances, null);

                var share = (float)marketShares[(sid.Individual, consumption.FoodConsumption.Food, consumption.FoodAsMeasured)];
                var intakesPerCompound = new List<DietaryIntakePerCompound>(concentrations.Count);
                var otherIntakesPerCompound = new List<DietaryIntakePerCompound>(concentrations.Count);
                var processingType = consumption.ProcessingTypes?.LastOrDefault();
                foreach (var concentration in concentrations) {
                    var reductionFactor = 1D;
                    if (isTdsReductionToLimitScenario) {
                        consumption.ConversionResultsPerCompound.TryGetValue(concentration.Compound, out var conversionResult);
                        reductionFactor = TdsReductionFactorsCalculator.GetReductionFactor(conversionResult, concentration.Compound, _tdsReductionFactors);
                    }
                    var processingFactor = 1D;
                    var proportionProcessing = 1F;
                    if (_processingFactorModels != null
                        && _processingFactorModels.TryGetProcessingFactorModel(consumption.FoodAsMeasured, concentration.Compound, processingType, out var processingFactorModel)) {
                        bool isApplyProcessingCorrectionFactor;
                        (processingFactor, isApplyProcessingCorrectionFactor) = processingFactorModel.DrawFromDistribution(processingFactorsRandomGenerator);
                        proportionProcessing = (float)(isApplyProcessingCorrectionFactor ? consumption.ProportionProcessing : 1D);
                    }
                    var ipc = new DietaryIntakePerCompound() {
                        IntakePortion = new IntakePortion() {
                            Amount = consumption.AmountFoodAsMeasured * share,
                            Concentration = concentration.Concentration * (float)reductionFactor,
                        },
                        Compound = concentration.Compound,
                        ProcessingCorrectionFactor = proportionProcessing,
                        ProcessingFactor = (float)processingFactor,
                        ProcessingType = processingType,
                    };
                    intakesPerCompound.Add(ipc);
                }

                if (processingType != null) {
                    intakesPerCompound
                        .Where(c => c.ProcessingType == null)
                        .ForAll(c => c.ProcessingType = processingType);
                }

                var intakePerFood = new IntakePerFood() {
                    Amount = consumption.AmountFoodAsMeasured * share,
                    ConsumptionFoodAsMeasured = consumption,
                    IntakesPerCompound = intakesPerCompound.Cast<IIntakePerCompound>().ToList(),
                };
                intakesPerFood.Add(intakePerFood);
            }

            var result = new DietaryIndividualDayIntake() {
                SimulatedIndividualId = sid.SimulatedIndividualId,
                SimulatedIndividualDayId = sid.SimulatedIndividualDayId,
                Individual = sid.Individual,
                Day = sid.Day,
                IndividualSamplingWeight = sid.IndividualSamplingWeight,
                IntakesPerFood = intakesPerFood.Cast<IIntakePerFood>().ToList(),
                OtherIntakesPerCompound = new List<IIntakePerCompound>(),
            };

            return result;
        }

        /// <summary>
        /// Get marketshare based on brand loyalty parameter L, note that shares are generated for an individual
        /// brandLoyalty = 0 is no brand loyalty, brandLoyalty = 1 is absolute brandloyalty
        /// </summary>
        /// <param name="marketShareRandomGenerator"></param>
        /// <param name="allConsumptions"></param>
        /// <returns></returns>
        private ConcurrentDictionary<(Individual, Food, Food), double> getMarketShares(IRandom marketShareRandomGenerator, List<ConsumptionsByModelledFood> allConsumptions) {
            var foodsAsEatenWithoutMarketShares = allConsumptions.Where(consumption => !consumption.IsBrand).ToList();
            foreach (var item in foodsAsEatenWithoutMarketShares) {
                _marketSharesDictionary.TryAdd((item.Individual, item.FoodConsumption.Food, item.FoodAsMeasured), 1D);
            }
            var foodsAsEatenWithMarketShares = allConsumptions.Where(consumption => consumption.IsBrand)
                .GroupBy(c => c.FoodConsumption)
                .Select(c => (
                    foodAsMeasured: c.Select(fam => fam.FoodAsMeasured).ToList(),
                    foodaseaten: c.Key.Food,
                    consumption: c.Key,
                    individual: c.Key.Individual
                )).Distinct(c => c.foodaseaten)
                .ToList();

            foreach (var item in foodsAsEatenWithMarketShares) {
                var marketShare = item.foodaseaten.MarketShare;
                var s = 1 / getBrandLoyalty(marketShare) - 1;

                //the scaling factor is used for marketshares that do not sum to a 100%;
                var scaling100 = 100 / item.foodAsMeasured.Select(food => food.MarketShare.Percentage).Sum();
                var probability = item.foodAsMeasured.Select(food => food.MarketShare.Percentage * s * scaling100).ToArray();
                var seed = marketShareRandomGenerator.Next(1, int.MaxValue);
                //random generator aanmaken
                lock (_marketShareLock) {
                    var shares = DirichletDistribution.Sample(probability, seed);
                    for (int i = 0; i < shares.Length; i++) {
                        _marketSharesDictionary.TryAdd((item.individual, item.foodaseaten, item.foodAsMeasured[i]), shares[i]);
                    }
                }
            }
            return _marketSharesDictionary;
        }

        /// <summary>
        /// get the right brandloyalty from marketShare
        /// </summary>
        /// <param name="marketShare"></param>
        /// <returns></returns>
        private double getBrandLoyalty(MarketShare marketShare) {
            var brandLoyalty = marketShare?.BrandLoyalty ?? 0.001;
            brandLoyalty = brandLoyalty < 0.001 ? 0.001 : brandLoyalty;
            brandLoyalty = brandLoyalty > 0.999 ? 0.999 : brandLoyalty;
            return brandLoyalty;
        }
    }
}

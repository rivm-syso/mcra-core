using log4net;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.MarketSharesCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.TdsReductionFactorsCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {
    public sealed class ChronicDietaryExposureCalculator : DietaryExposureCalculatorBase {

        private static readonly ILog log = LogManager.GetLogger(typeof(ChronicDietaryExposureCalculator));
        private readonly IResidueGenerator _residueGenerator;
        private readonly bool _isTotalDietStudy;
        private readonly bool _isScenarioAnalysis;
        private readonly IDictionary<(Food, Compound), double> _tdsReductionFactors;
        private Dictionary<(int, Food, Food), double> _individualMarketShares;

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

            // Consumption may contain brands (or marketshares) so select/draw the relevant foods, brands and shares
            _individualMarketShares = drawMarketShares(
                simulatedIndividualDays,
                randomSeed
            );

            var cancelToken = progressState?.CancellationToken ?? new CancellationToken();
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
                .Where(gr => !gr.Any(r => r.IntakesPerFood.Any(ipf => ipf.IntakesPerCompound.Any(ipc => ipc.Amount > 0))))
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
                                    Intake: g.Sum(ipc => ipc.Amount)
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

            // Some individual days have no consumption, socalled empty days.
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
            IRandom processingFactorsRandomGenerator;
            var random = new McraRandomGenerator(seed);
            processingFactorsRandomGenerator = new McraRandomGenerator(random.Next());

            if (!_consumptionsByFoodsAsMeasured.TryGetValue((sid.Individual, sid.Day), out var consumptions)) {
                consumptions = [];
            }

            var intakesPerFood = new List<IIntakePerFood>(consumptions.Count);
            foreach (var consumption in consumptions) {
                // For substance dependent food conversion paths, select the relevant substances based on conversion results, see issue #1090
                var concentrations = _isSubstanceDependent
                     ? _residueGenerator.GenerateResidues(consumption.FoodAsMeasured, _selectedSubstances.Intersect(consumption.ConversionResultsPerCompound.Keys).ToList(), null)
                     : _residueGenerator.GenerateResidues(consumption.FoodAsMeasured, _selectedSubstances, null);

                var share = _individualMarketShares[(
                    sid.SimulatedIndividualId,
                    consumption.FoodConsumption.Food,
                    consumption.FoodAsMeasured
                )];
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
        private Dictionary<(int, Food, Food), double> drawMarketShares(
            List<SimulatedIndividualDay> simulatedIndividualDays,
            int seed
        ) {
            var result = new Dictionary<(int, Food, Food), double>();
            var daysByIndividual = simulatedIndividualDays
                .GroupBy(r => r.SimulatedIndividualId);
            foreach (var group in daysByIndividual) {
                var consumptions = group
                    .SelectMany(r => _consumptionsByFoodsAsMeasured
                        .TryGetValue((r.Individual, r.Day), out var cons) ? cons : []
                    )
                    .ToList();

                var foodsAsEatenWithoutMarketShares = consumptions
                    .Where(consumption => !consumption.IsBrand)
                    .Distinct(r => (r.FoodAsEaten, r.FoodAsMeasured))
                    .ToList();
                foreach (var item in foodsAsEatenWithoutMarketShares) {
                    result.Add((group.Key, item.FoodConsumption.Food, item.FoodAsMeasured), 1D);
                }

                var foodsAsEatenWithMarketShares = consumptions
                    .Where(consumption => consumption.IsBrand)
                    .GroupBy(c => c.FoodConsumption)
                    .Select(c => (
                        foodAsEaten: c.Key.Food,
                        foodsAsMeasuredConversion: c
                            .Select(fam => (fam.FoodAsMeasured, fam.ConversionResultsPerCompound.First().Value.MarketShare))
                            .OrderBy(r => r.FoodAsMeasured.Code)
                            .ToList()
                    ))
                    .Distinct(c => c.foodAsEaten)
                    .ToList();

                foreach (var item in foodsAsEatenWithMarketShares) {
                    var brandLoyalty = item.foodsAsMeasuredConversion
                        .Select(r => r.FoodAsMeasured.MarketShare.BrandLoyalty)
                        .First();
                    var marketShares = item.foodsAsMeasuredConversion
                        .Select(r => r.MarketShare)
                        .ToList();
                    var individualMarketShares = MarketSharesCalculator
                        .SampleBrandLoyalty(
                            marketShares,
                            brandLoyalty,
                            RandomUtils.CreateSeed(seed, group.Key.ToString(), item.foodAsEaten.Code)
                        );
                    for (int i = 0; i < item.foodsAsMeasuredConversion.Count; i++) {
                        result.Add(
                            (group.Key, item.foodAsEaten, item.foodsAsMeasuredConversion[i].FoodAsMeasured),
                            individualMarketShares[i]
                        );
                    }
                }
            }

            return result;
        }
    }
}

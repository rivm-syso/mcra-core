using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.ConsumptionAmountGeneration;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation.ConsumptionUnitWeightGeneration;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.MaximalSubstanceIntakesCorrelationCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using MCRA.Utils.Statistics.RandomGenerators;
using MCRA.General;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {
    public sealed class AcuteDietaryExposureCalculator : DietaryExposureCalculatorBase {

        private readonly UnitVariabilityCalculator _unitVariabilityCalculator;
        private readonly IResidueGenerator _residueGenerator;

        private readonly int _numberOfMonteCarloIterations;
        private readonly bool _isSampleBased;
        private readonly bool _isCorrelation;
        private readonly bool _isSingleSamplePerDay;

        public AcuteDietaryExposureCalculator(
            ICollection<Compound> activeSubstances,
            IDictionary<(Individual, string), List<ConsumptionsByModelledFood>> consumptionsByFoodsAsMeasured,
            IProcessingFactorProvider processingFactorProvider,
            IIndividualDayIntakePruner individualDayIntakePruner,
            IResidueGenerator residueGenerator,
            UnitVariabilityCalculator unitVariabilityCalculator,
            ICollection<ConsumptionsByModelledFood> consumptionsByModelledFood,
            int numberOfMonteCarloIterations,
            bool isSampleBased,
            bool isCorrelation,
            bool isSingleSamplePerDay) : base(
            consumptionsByFoodsAsMeasured,
            processingFactorProvider,
            activeSubstances,
            individualDayIntakePruner
        ) {
            _unitVariabilityCalculator = unitVariabilityCalculator;
            _numberOfMonteCarloIterations = numberOfMonteCarloIterations;
            _consumptionsByFoodsAsMeasured = consumptionsByFoodsAsMeasured;
            _isSampleBased = isSampleBased;
            _isCorrelation = isCorrelation;
            _isSingleSamplePerDay = isSingleSamplePerDay;
            _residueGenerator = residueGenerator;
            if (UnitWeightGenerator == null) {
                UnitWeightGenerator = new ConsumptionUnitWeightGenerator();
                consumptionsByModelledFood
                    .Select(c => c.FoodConsumption.FoodConsumptionQuantification)
                    .Distinct()
                    .ForAll(fcq => UnitWeightGenerator.GetOrCreateModel(fcq));
            }
        }

        /// <summary>
        /// Simulates the acute individual days.
        /// </summary>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public override List<DietaryIndividualDayIntake> CalculateDietaryIntakes(
            List<SimulatedIndividualDay> simulatedIndividualDays,
            ProgressState progressState,
            int randomSeed
        ) {
            int partitionSize = 1000;
            var cancelToken = progressState?.CancellationToken ?? new System.Threading.CancellationToken();

            // Calculate the individual day exposures parallel, unitvariability is implemented within the individualday calculation
            var result = simulatedIndividualDays.Partition(partitionSize)
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(partition => {
                    var dietaryIntakes = new List<DietaryIndividualDayIntake>(partitionSize);
                    foreach (var individualDay in partition) {
                        var seed = RandomUtils.CreateSeed(randomSeed, individualDay.SimulatedIndividualId);
                        var dietaryIndividualDayIntake = calculateIndividualDayIntake(individualDay, seed);
                        var prunedIndividualDayIntake = (_individualDayIntakePruner != null)
                            ? _individualDayIntakePruner?.Prune(dietaryIndividualDayIntake)
                            : dietaryIndividualDayIntake;
                        dietaryIntakes.Add(prunedIndividualDayIntake);
                    }
                    return dietaryIntakes;
                })
                .ToList();

            var dietaryIndividualDayIntakes = new List<DietaryIndividualDayIntake>(_numberOfMonteCarloIterations);
            dietaryIndividualDayIntakes = result.SelectMany(l => l).ToList();

            if (!_isSampleBased && _isCorrelation) {
                if (_individualDayIntakePruner != null) {
                    // Note that this method does not work together with individual day pruning
                    throw new NotImplementedException("Cannot maximise co-occurrence of high substance concentration values in simulated intakes when omitting details of the simulated dietary exposures.");
                }
                MaximalSubstanceIntakesCorrelationCalculator.Compute(
                    dietaryIndividualDayIntakes,
                    progressState
                );
            }
            return dietaryIndividualDayIntakes;
        }

        /// <summary>
        /// Override: compute the exposures per substance.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <returns></returns>
        public override Dictionary<Compound, List<ExposureRecord>> ComputeExposurePerCompoundRecords(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            var exposurePerCompoundRecords = new Dictionary<Compound, List<ExposureRecord>>();
            var emptyDayIntakes = new List<ExposureRecord>();

            foreach (var idi in dietaryIndividualDayIntakes) {
                var intakeSumsPerCompound = new Dictionary<Compound, double>();
                foreach (var ipc in idi.IntakesPerFood.SelectMany(f => f.IntakesPerCompound)) {
                    intakeSumsPerCompound.TryGetValue(ipc.Compound, out var exposure);
                    intakeSumsPerCompound[ipc.Compound] = exposure + ipc.Amount;
                }

                if (intakeSumsPerCompound.Any()) {
                    foreach (var kvp in intakeSumsPerCompound) {
                        if (!exposurePerCompoundRecords.TryGetValue(kvp.Key, out var exposures)) {
                            exposures = [];
                            exposurePerCompoundRecords.Add(kvp.Key, exposures);
                        }
                        exposures.Add(new ExposureRecord {
                            IndividualDayId = idi.SimulatedIndividualDayId,
                            Exposure = kvp.Value,
                            BodyWeight = idi.Individual.BodyWeight,
                            SamplingWeight = idi.IndividualSamplingWeight
                        });
                    }
                } else {
                    emptyDayIntakes.Add(new ExposureRecord {
                        IndividualDayId = idi.SimulatedIndividualDayId,
                        Exposure = 0,
                        BodyWeight = idi.Individual.BodyWeight,
                        SamplingWeight = idi.IndividualSamplingWeight
                    });
                }
            }
            return exposurePerCompoundRecords;
        }

        /// <summary>
        /// Calculate the individual-day intakes.
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private DietaryIndividualDayIntake calculateIndividualDayIntake(
            SimulatedIndividualDay sid,
            int seed
        ) {
            IRandom marketSharesRandomGenerator;
            IRandom portionUnitWeightRandomGenerator;
            IRandom consumptionAmountRandomGenerator;
            IRandom processingFactorsRandomGenerator;
            IRandom residueGeneratorRandomGenerator;
            IRandom unitVariabilityRandomGenerator;
            var random = new McraRandomGenerator(seed);
            Func<RandomSource, IRandom> spawnGenerator = x => new McraRandomGenerator(RandomUtils.CreateSeed(seed, (int)x));
            unitVariabilityRandomGenerator = spawnGenerator(RandomSource.DE_DrawUnitVariabilityFactor);
            marketSharesRandomGenerator = spawnGenerator(RandomSource.DE_DrawMarketShare);
            portionUnitWeightRandomGenerator = spawnGenerator(RandomSource.DE_DrawUnitWeight);
            consumptionAmountRandomGenerator = spawnGenerator(RandomSource.DE_DrawConsumptionAmount);
            processingFactorsRandomGenerator = spawnGenerator(RandomSource.DE_DrawProcessingFactor);
            residueGeneratorRandomGenerator = spawnGenerator(RandomSource.DE_DrawConcentration);

            // Collect the consumptions of the simulated individual
            if (_consumptionsByFoodsAsMeasured.TryGetValue((sid.Individual, sid.Day), out var allConsumptions)) {
                allConsumptions = allConsumptions
                  .OrderBy(c => c.ConversionResultsPerCompound.First().Value.AllStepsToMeasuredString, StringComparer.OrdinalIgnoreCase)
                  .ThenBy(c => c.FoodConsumption.Amount)
                  .ToList();
            } else {
                allConsumptions = [];
            }

            // Consumption may contain brands (or marketshares) so select/draw the relevant foods and brands
            var consumptions = getConsumptions(marketSharesRandomGenerator, allConsumptions);
            var amountGenerator = new ConsumptionAmountGenerator();

            // Compute the exposure per consumption
            var intakesPerFood = new List<IntakePerFood>(consumptions.Count);
            var concentrationDict = new Dictionary<Food, List<CompoundConcentration>>();
            foreach (var consumption in consumptions) {
                var portionUnitWeight = UnitWeightGenerator
                    .GenerateUnitWeight(consumption, portionUnitWeightRandomGenerator);
                var amountFactor = ModelConsumptionAmountUncertainty
                    ? amountGenerator.GenerateAmountFactor(consumption, consumptionAmountRandomGenerator)
                    : 1D;
                var consumedAmount = consumption.AmountFoodAsMeasured * amountFactor * portionUnitWeight;

                // Sample for multiple consumed modelled food identical concentrations (see guidance, EFSA pessimistic)
                var concentrations = new List<CompoundConcentration>();

                if (!_isSingleSamplePerDay
                    || !concentrationDict.TryGetValue(consumption.FoodAsMeasured, out concentrations)
                ) {
                    // Sample new concentrations for each consumption (see guidance, EFSA optimistic)
                    // For substance dependent food conversion paths, select the relevant substances based on conversion results, see issue #1090
                    var substances = _isSubstanceDependent
                                   ? _selectedSubstances.Intersect(consumption.ConversionResultsPerCompound.Keys).ToList()
                                   : _selectedSubstances;
                    concentrations = _residueGenerator.GenerateResidues(
                        consumption.FoodAsMeasured,
                        substances,
                        residueGeneratorRandomGenerator
                    );

                    // Only add to dict if necessary
                    if (_isSingleSamplePerDay) {
                        concentrationDict.Add(consumption.FoodAsMeasured, concentrations);
                    }
                }

                var intakesPerCompound = new List<DietaryIntakePerCompound>(concentrations.Count);
                var processingType = consumption.ProcessingTypes?.LastOrDefault();
                foreach (var concentration in concentrations) {
                    var processingFactor = 1D;
                    var proportionProcessing = 1F;
                    if (processingType != null 
                        && _processingFactorProvider != null
                        && _processingFactorProvider
                            .HasProcessingFactor(consumption.FoodAsMeasured, concentration.Compound, processingType)
                    ) {
                        processingFactor = _processingFactorProvider
                            .GetProcessingFactor(
                                consumption.FoodAsMeasured,
                                concentration.Compound,
                                processingType,
                                processingFactorsRandomGenerator
                            );
                        var applyProcessingCorrectionFactor = _processingFactorProvider.GetProportionProcessingApplication(
                            consumption.FoodAsMeasured,
                            concentration.Compound,
                            processingType
                        );
                        proportionProcessing = (float)(applyProcessingCorrectionFactor ? consumption.ProportionProcessing : 1D);
                    }
                    var ipc = new DietaryIntakePerCompound() {
                        IntakePortion = new IntakePortion() {
                            Amount = consumption.AmountFoodAsMeasured,
                            Concentration = concentration.Concentration
                        },
                        ProcessingCorrectionFactor = proportionProcessing,
                        ProcessingFactor = (float)processingFactor,
                        ProcessingType = processingType,
                        Compound = concentration.Compound,
                    };
                    intakesPerCompound.Add(ipc);
                }

                if (_unitVariabilityCalculator != null) {
                    intakesPerCompound = _unitVariabilityCalculator.CalculateResidues(intakesPerCompound, consumption.FoodAsMeasured, unitVariabilityRandomGenerator);
                }

                var intakePerFood = new IntakePerFood() {
                    Amount = consumedAmount,
                    ConsumptionFoodAsMeasured = consumption,
                    IntakesPerCompound = intakesPerCompound.Cast<IIntakePerCompound>().ToList(),
                };
                intakesPerFood.Add(intakePerFood);
            }
            var result = new DietaryIndividualDayIntake() {
                SimulatedIndividualId = sid.Individual.Id,
                SimulatedIndividualDayId = sid.SimulatedIndividualDayId,
                IndividualSamplingWeight = sid.IndividualSamplingWeight,
                Day = sid.Day,
                Individual = sid.Individual,
                IntakesPerFood = intakesPerFood.Cast<IIntakePerFood>().ToList(),
                OtherIntakesPerCompound = [],
            };
            return result;
        }

        /// <summary>
        /// 1) Get consumptions for all foods without marketshares IsBrand == false
        /// 2) Then, sample a Brand based on marketshare whenever this is needed
        /// </summary>
        /// <param name="marketShareRandomGenerator"></param>
        /// <param name="allConsumptions"></param>
        /// <returns></returns>
        private List<ConsumptionsByModelledFood> getConsumptions(
            IRandom marketShareRandomGenerator,
            List<ConsumptionsByModelledFood> allConsumptions
        ) {
            var consumptions = new List<ConsumptionsByModelledFood>();
            consumptions.AddRange(allConsumptions.Where(consumption => !consumption.IsBrand).ToList());
            if (consumptions.Count != allConsumptions.Count) {
                var foodsAsEatenWithMarketShares = allConsumptions
                    .Where(consumption => consumption.IsBrand)
                    .GroupBy(c => c.FoodConsumption)
                    .Select(c => (
                        famRecords: c
                            .Select(fam => new {
                                FoodAsMeasured = fam.FoodAsMeasured,
                                MarketShare = fam.ConversionResultsPerCompound.First().Value.MarketShare
                            }),
                        consumption: c.Key
                    ))
                    .ToList();
                foreach (var item in foodsAsEatenWithMarketShares) {
                    var selectedBrand = item.famRecords
                        .DrawRandom(
                            marketShareRandomGenerator,
                            r => r.MarketShare
                        )?.FoodAsMeasured;
                    if (selectedBrand != null) {
                        var selectedConsumption = allConsumptions
                            .First(c => c.FoodConsumption == item.consumption && c.FoodAsMeasured == selectedBrand);
                        consumptions.Add(selectedConsumption);
                    }
                }
            }
            return consumptions;
        }
    }
}

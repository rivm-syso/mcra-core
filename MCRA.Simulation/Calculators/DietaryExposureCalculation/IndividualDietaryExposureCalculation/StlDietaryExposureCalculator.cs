using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation {
    public sealed class StlDietaryExposureCalculator : DietaryExposureCalculatorBase {

        private readonly IResidueGenerator _residueGenerator;

        private readonly int _stlNumberOfDays;

        public StlDietaryExposureCalculator(
            ICollection<Compound> activeSubstances,
            IDictionary<(Individual, string), List<ConsumptionsByModelledFood>> consumptionsByFoodsAsMeasured,
            IIndividualDayIntakePruner individualDayIntakePruner,
            IProcessingFactorProvider processingFactorProvider,
            IResidueGenerator residueGenerator,
            int stlNumberOfDays
        ) : base(
              consumptionsByFoodsAsMeasured,
              processingFactorProvider,
              activeSubstances,
              individualDayIntakePruner
        ) {
            _stlNumberOfDays = stlNumberOfDays;
            _consumptionsByFoodsAsMeasured = consumptionsByFoodsAsMeasured;
            _residueGenerator = residueGenerator;
        }

        /// <summary>
        /// Calculate STL dietary daily intakes
        /// </summary>
        public override List<DietaryIndividualDayIntake> CalculateDietaryIntakes(
            List<SimulatedIndividualDay> simulatedIndividualDays,
            ProgressState progressState,
            int randomSeed
        ) {
            void logProgress(string message, double percentCompleted) {
                progressState.CurrentActivity = message;
                progressState.Progress = percentCompleted;
            }

            logProgress("Calculating individual-day exposures", 50);

            var ix = 0;
            var resampledIndividualDays = simulatedIndividualDays
                .GroupBy(r => r.SimulatedIndividual)
                .SelectMany(ind => Enumerable
                    .Repeat(0, _stlNumberOfDays)
                    .Select(r => new SimulatedIndividualDay(ind.Key) {
                        SimulatedIndividualDayId = ix++
                    })
                    .ToList()
                )
                .GroupBy(r => r.SimulatedIndividual);

            var individualDaysLookup = simulatedIndividualDays
                .ToLookup(r => r.SimulatedIndividual);

            var cancelToken = progressState?.CancellationToken ?? new();
            var result = resampledIndividualDays
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(ind => {
                    var seed = randomSeed + ind.Key.Id;
                    var rnd = new McraRandomGenerator(seed);
                    var candidates = individualDaysLookup[ind.Key].ToList();
                    var intakes = ind
                        .Select(r => {
                            var sample = candidates[rnd.Next(candidates.Count)];
                            var individualDayIntake = calculateIndividualDayIntake(
                                r,
                                ind.Key.Individual,
                                sample.Day,
                                seed
                            );
                            var prunedIndividualDayIntake = (_individualDayIntakePruner != null)
                                ? _individualDayIntakePruner?.Prune(individualDayIntake)
                                : individualDayIntake;
                            return prunedIndividualDayIntake;
                        })
                        .ToList();
                    return intakes;
                });

            logProgress("Finished calculating dietary individual-day exposures", 100);
            return result.OrderBy(c => c.SimulatedIndividualDayId).ToList();
        }

        /// <summary>
        /// Override: compute the exposures per substance.
        /// </summary>
        public override Dictionary<Compound, List<ExposureRecord>> ComputeExposurePerCompoundRecords(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            return null;
        }

        private DietaryIndividualDayIntake calculateIndividualDayIntake(
            SimulatedIndividualDay sid,
            Individual individual,
            string day,
            int seed
        ) {
            Func<RandomSource, IRandom> spawnGenerator = x => new McraRandomGenerator(RandomUtils.CreateSeed(seed, (int)x));
            var processingFactorsRandomGenerator = spawnGenerator(RandomSource.DE_DrawProcessingFactor);
            var residueGeneratorRandomGenerator = spawnGenerator(RandomSource.DE_DrawConcentration);

            if (!_consumptionsByFoodsAsMeasured.TryGetValue((individual, day), out var consumptions)) {
                consumptions = [];
            }

            var intakesPerFood = new List<IIntakePerFood>(consumptions.Count);
            foreach (var consumption in consumptions) {
                // For substance dependent food conversion paths, select the relevant substances based on conversion results, see issue #1090
                var concentrations = _isSubstanceDependent
                     ? _residueGenerator.GenerateResidues(consumption.FoodAsMeasured, _selectedSubstances.Intersect(consumption.ConversionResultsPerCompound.Keys).ToList(), residueGeneratorRandomGenerator)
                     : _residueGenerator.GenerateResidues(consumption.FoodAsMeasured, _selectedSubstances, residueGeneratorRandomGenerator);

                var intakesPerCompound = new List<DietaryIntakePerCompound>(concentrations.Count);
                var otherIntakesPerCompound = new List<DietaryIntakePerCompound>(concentrations.Count);
                var processingType = consumption.ProcessingTypes?.LastOrDefault();
                foreach (var concentration in concentrations) {
                    var reductionFactor = 1D;
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
                    Amount = consumption.AmountFoodAsMeasured,
                    ConsumptionFoodAsMeasured = consumption,
                    IntakesPerCompound = intakesPerCompound.Cast<IIntakePerCompound>().ToList(),
                };
                intakesPerFood.Add(intakePerFood);
            }

            var result = new DietaryIndividualDayIntake() {
                SimulatedIndividualDayId = sid.SimulatedIndividualDayId,
                SimulatedIndividual = sid.SimulatedIndividual,
                Day = sid.Day,
                IntakesPerFood = intakesPerFood.Cast<IIntakePerFood>().ToList(),
                OtherIntakesPerCompound = [],
            };

            return result;
        }
    }
}

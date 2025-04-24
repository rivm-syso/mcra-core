using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDayPruning {

    /// <summary>
    /// Individual day intake pruner that prunes (removed) the non-selected SCCs, and records the
    /// combined intake of these pruned non-selected SCCs into aggregate intakes per modelled food
    /// objects. I.e., intakes of the non-selected SCCs are aggregated per food-as-measured / compound.
    /// </summary>
    public sealed class ScreeningToAggregateIntakesPruner : IIndividualDayIntakePruner {

        private readonly HashSet<Tuple<Food, Food>> _screeningFamFaeCombinations = null;
        private readonly HashSet<Tuple<Food, Food, Compound>> _screeningFamFaeCompoundCombinations = null;
        private readonly IDictionary<Compound, double> _relativePotencyFactors;
        private readonly IDictionary<Compound, double> _membershipProbabilities;

        public ScreeningToAggregateIntakesPruner(
            List<GroupedScreeningResultRecord> GroupedScreeningResults,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            if (GroupedScreeningResults != null) {
                _screeningFamFaeCombinations = GroupedScreeningResults
                    .SelectMany(c => c.ScreeningRecords,
                        (c, sr) => new Tuple<Food, Food>(c.FoodAsMeasured, sr.FoodAsEaten))
                    .ToHashSet();
                _screeningFamFaeCompoundCombinations = GroupedScreeningResults
                    .SelectMany(c => c.ScreeningRecords,
                        (c, sr) => new Tuple<Food, Food, Compound>(c.FoodAsMeasured, sr.FoodAsEaten, c.Compound))
                    .ToHashSet();
            }
            _relativePotencyFactors = relativePotencyFactors;
            _membershipProbabilities = membershipProbabilities;
        }

        public DietaryIndividualDayIntake Prune(
            DietaryIndividualDayIntake dietaryIndividualDayIntake
        ) {
            if (_screeningFamFaeCombinations == null) {
                return dietaryIndividualDayIntake;
            }

            var intakesPerFood = new List<IIntakePerFood>();
            var others = new List<AggregateIntakePerFood>();
            foreach (var ipf in dietaryIndividualDayIntake.DetailedIntakesPerFood) {
                var otherIntakesPerCompound = new List<IIntakePerCompound>();
                var intakesPerCompoundScreening = new List<IIntakePerCompound>();
                foreach (var ipc in ipf.IntakesPerCompound) {
                    if (_screeningFamFaeCompoundCombinations.Contains(new Tuple<Food, Food, Compound>(ipf.FoodAsMeasured, ipf.FoodConsumption.Food, ipc.Compound))) {
                        intakesPerCompoundScreening.Add(ipc);
                    } else {
                        otherIntakesPerCompound.Add(ipc);
                    }
                }

                var actualGrossAmount = ipf.Amount;
                var actualNetAmount = ipf.Intake(_relativePotencyFactors, _membershipProbabilities) > 0 ? ipf.Amount : 0;
                var detailedGrossAmount = intakesPerCompoundScreening.Any() ? ipf.Amount : 0;
                var detailedNetAmount = intakesPerCompoundScreening.Any(ipc => ipc.EquivalentSubstanceAmount(_relativePotencyFactors[ipc.Compound], _membershipProbabilities[ipc.Compound]) > 0) ? ipf.Amount : 0;
                var aggregateGrossAmount = actualGrossAmount - detailedGrossAmount;
                var aggregateNetAmount = actualNetAmount - detailedNetAmount;

                if (otherIntakesPerCompound.Any()) {
                    others.Add(new AggregateIntakePerFood() {
                        FoodAsMeasured = ipf.FoodAsMeasured,
                        NetAmount = aggregateNetAmount,
                        GrossAmount = aggregateGrossAmount,
                        IntakesPerCompound = otherIntakesPerCompound,
                    });
                }
                if (_screeningFamFaeCombinations.Contains(new Tuple<Food, Food>(ipf.FoodAsMeasured, ipf.ConsumptionFoodAsMeasured.FoodAsEaten))) {
                    intakesPerCompoundScreening.TrimExcess();
                    var intakePerFood = new IntakePerFood() {
                        Amount = ipf.Amount,
                        ConsumptionFoodAsMeasured = ipf.ConsumptionFoodAsMeasured,
                        IntakesPerCompound = intakesPerCompoundScreening,
                    };
                    intakesPerFood.Add(intakePerFood);
                }
            }

            var othersGrouped = others
                .GroupBy(ipf => ipf.FoodAsMeasured)
                .Select(ipf => {
                    var intakesPerCompound = ipf
                        .SelectMany(ipc => ipc.IntakesPerCompound)
                        .GroupBy(ipc => ipc.Compound)
                        .Select(ipcg => new AggregateIntakePerCompound() {
                            Compound = ipcg.Key,
                            Amount = ipcg.Sum(i => i.Amount)
                        })
                        .Cast<IIntakePerCompound>()
                        .ToList();
                    var intake = intakesPerCompound
                        .Sum(r => r.EquivalentSubstanceAmount(_relativePotencyFactors[r.Compound], _membershipProbabilities[r.Compound]));
                    var record = new AggregateIntakePerFood() {
                        FoodAsMeasured = ipf.Key,
                        GrossAmount = ipf.Sum(g => g.GrossAmount),
                        NetAmount = ipf.Sum(g => g.NetAmount),
                        BodyWeight = dietaryIndividualDayIntake.SimulatedIndividual.BodyWeight,
                        IntakesPerCompound = intakesPerCompound,
                    };
                    return record;
                })
                .Cast<IIntakePerFood>();

            intakesPerFood.AddRange(othersGrouped);
            intakesPerFood.TrimExcess();

            var result = new DietaryIndividualDayIntake() {
                SimulatedIndividualDayId = dietaryIndividualDayIntake.SimulatedIndividualDayId,
                SimulatedIndividual = dietaryIndividualDayIntake.SimulatedIndividual,
                Day = dietaryIndividualDayIntake.Day,
                IntakesPerFood = intakesPerFood,
            };
            return result;
        }
    }
}

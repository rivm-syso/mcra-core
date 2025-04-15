//using System;
//using System.Collections.Generic;
//using System.Linq;
//using MCRA.Data.Compiled.Objects;
//using MCRA.Simulation.Objects;
//using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

//namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning {

//    /// <summary>
//    /// Individual day intake pruner that prunes (removed) the non-selected SCCs, and records the
//    /// combined intake of these pruned non-selected SCCs into a single value within the individual
//    /// day intake object, which is OthersDietaryIntakePerBodyWeight.
//    /// </summary>
//    public sealed class ScreeningToSingleIntakePruner : IIndividualDayIntakePruner {

//        private HashSet<Tuple<Food, Food>> _screeningFamFaeCombinations = null;
//        private HashSet<Tuple<Food, Food, Compound>> _screeningFamFaeCompoundCombinations = null;

//        public ScreeningToSingleIntakePruner(List<GroupedScreeningResultRecord> GroupedScreeningResults) {
//            if (GroupedScreeningResults != null) {
//                _screeningFamFaeCombinations = new HashSet<Tuple<Food, Food>>(GroupedScreeningResults
//                    .SelectMany(c => c.ScreeningRecords,
//                    (c, sr) => new Tuple<Food, Food>(c.FoodAsMeasured, sr.FoodAsEaten)));
//                _screeningFamFaeCompoundCombinations = new HashSet<Tuple<Food, Food, Compound>>(GroupedScreeningResults
//                    .SelectMany(c => c.ScreeningRecords,
//                    (c, sr) => new Tuple<Food, Food, Compound>(c.FoodAsMeasured, sr.FoodAsEaten, c.Compound)));
//            }
//        }

//        public DietaryIndividualDayIntake Prune(DietaryIndividualDayIntake dietaryIndividualDayIntake, IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
//            if (_screeningFamFaeCombinations == null) {
//                return dietaryIndividualDayIntake;
//            }

//            var othersDietaryIntakePerBodyWeight = 0D;
//            var intakesPerFood = new List<IIntakePerFood>();
//            foreach (var ipf in dietaryIndividualDayIntake.IntakesPerFood.Cast<IntakePerFood>()) {
//                var intakesPerCompoundScreening = new List<IIntakePerCompound>();
//                var otherIntakesPerCompound = new List<IIntakePerCompound>();
//                foreach (var ipc in ipf.IntakesPerCompound) {
//                    if (_screeningFamFaeCompoundCombinations.Contains(new Tuple<Food, Food, Compound>(ipf.FoodAsMeasured, ipf.FoodConsumption.Food, ipc.Compound))) {
//                        intakesPerCompoundScreening.Add(ipc);
//                    } else {
//                        otherIntakesPerCompound.Add(ipc);
//                    }
//                }

//                if (_screeningFamFaeCombinations.Contains(new Tuple<Food, Food>(ipf.FoodAsMeasured, ipf.Consumption.FoodAsEaten))) {
//                    intakesPerCompoundScreening.TrimExcess();
//                    var intakePerFood = new IntakePerFood() {
//                        Amount = ipf.Amount,
//                        Consumption = ipf.Consumption,
//                        IntakesPerCompound = intakesPerCompoundScreening,
//                        Intake = intakesPerCompoundScreening.Sum(r => r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]))
//                    };
//                    intakesPerFood.Add(intakePerFood);
//                }

//                othersDietaryIntakePerBodyWeight += (new IntakePerFood() {
//                    Amount = ipf.Amount,
//                    Consumption = ipf.Consumption,
//                    IntakesPerCompound = otherIntakesPerCompound,
//                }).IntakePerBodyWeight;
//            }
//            intakesPerFood.TrimExcess();

//            var result = new DietaryIndividualDayIntake() {
//                SimulatedIndividualId = dietaryIndividualDayIntake.SimulatedIndividualId,
//                SimulatedIndividualDayId = dietaryIndividualDayIntake.SimulatedIndividualDayId,
//                Day = dietaryIndividualDayIntake.Day,
//                IndividualSamplingWeight = dietaryIndividualDayIntake.SimulatedIndividual.SamplingWeight,
//                Individual = dietaryIndividualDayIntake.Individual,
//                IntakesPerFood = intakesPerFood,
//                DietaryAbsorptionFactor = dietaryIndividualDayIntake.DietaryAbsorptionFactor,
//                OthersDietaryInTakePerBodyWeight = othersDietaryIntakePerBodyWeight,
//            };
//            return result;
//        }
//    }
//}

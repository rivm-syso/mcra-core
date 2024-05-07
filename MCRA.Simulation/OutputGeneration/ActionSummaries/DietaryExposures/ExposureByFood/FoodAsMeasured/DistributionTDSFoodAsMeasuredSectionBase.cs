using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionTDSFoodAsMeasuredSectionBase : SummarySection {
        public double _lowerPercentage;
        public double _upperPercentage;
        /// <summary>
        /// Does not exist for TDS, however this is the acute one
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public List<TDSReadAcrossFoodRecord> SummarizeAcute(
                ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                ICollection<Compound> selectedCompounds,
                bool isPerPerson
            ) {
            var totalDietaryIntake = dietaryIndividualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.IndividualSamplingWeight);
            var tdsExposure = 0D;
            var readAcrossFoods = new List<Info>();
            foreach (var intake in dietaryIndividualDayIntakes) {
                var samplingWeight = intake.IndividualSamplingWeight;
                var intakesPerFood = intake.DetailedIntakesPerFood;
                var bodyWeight = intake.Individual.BodyWeight;
                foreach (var compound in selectedCompounds) {
                    foreach (var intakePerFood in intakesPerFood) {
                        if (intakePerFood.ConsumptionFoodAsMeasured.ConversionResultsPerCompound.ContainsKey(compound)) {
                            var foodConversionResult = intakePerFood.ConsumptionFoodAsMeasured.ConversionResultsPerCompound[compound];
                            var isReadAcross = foodConversionResult.ConversionStepResults.Any(r => r.Step == FoodConversionStepType.ReadAcross);
                            var exposureForCompound = (intakePerFood.IntakesPerCompound.First(ipc => ipc.Compound == compound).EquivalentSubstanceAmount(relativePotencyFactors[compound], membershipProbabilities[compound]) / (isPerPerson ? 1 : bodyWeight)) * samplingWeight;
                            if (isReadAcross) {
                                if (!readAcrossFoods.Select(c => c.Food).Contains(intakePerFood.FoodAsMeasured)) {
                                    readAcrossFoods.Add(new Info() {
                                        Food = intakePerFood.FoodAsMeasured,
                                        Contribution = exposureForCompound,
                                        TDSFood = foodConversionResult.FoodAsMeasured,
                                    });
                                } else {
                                    readAcrossFoods.First(c => c.Food == intakePerFood.FoodAsMeasured).Contribution += exposureForCompound;
                                }
                            } else {
                                tdsExposure += exposureForCompound;
                            }
                        }
                    }
                }
            }

            var tdsContribution = tdsExposure / totalDietaryIntake;
            var totalDistributionTDSFoodAsMeasuredRecords = new List<TDSReadAcrossFoodRecord> {
                new TDSReadAcrossFoodRecord() {
                    Contribution = tdsContribution,
                    FoodName = "All TDS samples",
                    Translation = "Composition",
                    Contributions = new List<double>(),
                }
            };
            var resultReadAcrossFoods = readAcrossFoods.OrderByDescending(c => c.Contribution).ToList();
            foreach (var item in resultReadAcrossFoods) {
                totalDistributionTDSFoodAsMeasuredRecords.Add(new TDSReadAcrossFoodRecord() {
                    Contribution = item.Contribution / totalDietaryIntake,
                    FoodCode = item.Food.Code,
                    FoodName = item.Food.Name,
                    Translation = "Read Across",
                    TDSFoodName = item.TDSFood.Name,
                    TDSFoodCode = item.TDSFood.Code,
                    Contributions = new List<double>(),
                });
            }
            return totalDistributionTDSFoodAsMeasuredRecords;
        }

        /// <summary>
        /// Probably it doesn't matter acute or chronic because everything is accumulated
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public List<TDSReadAcrossFoodRecord> SummarizeChronic(
                ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                ICollection<Compound> selectedCompounds,
                bool isPerPerson
            ) {
            var tdsExposure = 0D;
            var readAcrossFoods = new List<Info>();
            var tmp = new List<InfoTmp>();
            foreach (var intake in dietaryIndividualDayIntakes) {
                var samplingWeight = intake.IndividualSamplingWeight;
                var intakesPerFood = intake.DetailedIntakesPerFood;
                var bodyWeight = intake.Individual.BodyWeight;
                foreach (var compound in selectedCompounds) {
                    foreach (var intakePerFood in intakesPerFood) {
                        if (intakePerFood.ConsumptionFoodAsMeasured.ConversionResultsPerCompound.ContainsKey(compound)) {
                            var foodConversionResult = intakePerFood.ConsumptionFoodAsMeasured.ConversionResultsPerCompound[compound];
                            var isReadAcross = foodConversionResult.ConversionStepResults.Any(r => r.Step == FoodConversionStepType.ReadAcross);
                            var exposureForCompound = (intakePerFood.IntakesPerCompound.FirstOrDefault(ipc => ipc.Compound == compound)?.EquivalentSubstanceAmount(relativePotencyFactors[compound], membershipProbabilities[compound]) / (isPerPerson ? 1 : bodyWeight)) * samplingWeight ?? 0;
                            tmp.Add(new InfoTmp() {
                                Food = intakePerFood.FoodAsEaten,
                                Contribution = exposureForCompound,
                                SimulatedIndividualDayId = intake.SimulatedIndividualDayId,
                                SimulatedIndividualId = intake.SimulatedIndividualId,
                                TranslationType = isReadAcross ? TranslationType.ReadAcross : TranslationType.Composition,
                                TDSFood = foodConversionResult.FoodAsMeasured,
                            });
                        }
                    }
                }
            }

            var perFoodTmp = tmp
                .GroupBy(gr => (gr.SimulatedIndividualDayId, gr.Food))
                .Select(c => (
                    Contribution: c.Sum(a => a.Contribution),
                    Food: c.First().Food,
                    TranslationType: c.First().TranslationType,
                    TdsFood: c.First().TDSFood,
                    IndividualId: c.First().SimulatedIndividualId
                ))
            .GroupBy(gr => (gr.IndividualId, gr.Food))
            .Select(c => (
                exposureForCompound: c.Sum(a => a.Contribution),
                Food: c.First().Food,
                TranslationType: c.First().TranslationType,
                tdsFood: c.First().TdsFood,
                IndividualId: c.First().IndividualId
            ))
            .ToList();

            foreach (var item in perFoodTmp) {
                if (item.TranslationType == TranslationType.Composition) {
                    tdsExposure += item.exposureForCompound;
                } else if (item.TranslationType == TranslationType.ReadAcross) {
                    if (!readAcrossFoods.Select(c => c.Food).Contains(item.Food)) {
                        readAcrossFoods.Add(new Info() {
                            Food = item.Food,
                            Contribution = item.exposureForCompound,
                            TDSFood = item.tdsFood,
                        });
                    } else {
                        readAcrossFoods.First(c => c.Food == item.Food).Contribution += item.exposureForCompound;
                    }
                }
            }
            var totalDietaryIntake = perFoodTmp.Sum(c => c.exposureForCompound);
            var totalDistributionTDSFoodAsMeasuredRecords = new List<TDSReadAcrossFoodRecord> {
                new TDSReadAcrossFoodRecord() {
                    Contribution = tdsExposure / totalDietaryIntake,
                    FoodName = "All TDS samples",
                    Translation = "Composition",
                    Contributions = new List<double>(),
                }
            };
            var resultReadAcrossFoods = readAcrossFoods.OrderByDescending(c => c.Contribution).ToList();
            foreach (var item in resultReadAcrossFoods) {
                totalDistributionTDSFoodAsMeasuredRecords.Add(new TDSReadAcrossFoodRecord() {
                    Contribution = item.Contribution / totalDietaryIntake,
                    FoodCode = item.Food.Code,
                    FoodName = item.Food.Name,
                    Translation = "Read Across",
                    TDSFoodName = item.TDSFood.Name,
                    TDSFoodCode = item.TDSFood.Code,
                    Contributions = new List<double>(),
                });
            }
            totalDistributionTDSFoodAsMeasuredRecords.TrimExcess();
            return totalDistributionTDSFoodAsMeasuredRecords;

        }

        private sealed class Info {
            public Food Food { get; set; }
            public double Contribution { get; set; }
            public Food TDSFood { get; set; }
        }

        private sealed class InfoTmp {
            public Food Food { get; set; }
            public double Contribution { get; set; }
            public Food TDSFood { get; set; }
            public int SimulatedIndividualDayId { get; set; }
            public int SimulatedIndividualId { get; set; }
            public TranslationType TranslationType { get; set; }
        }
    }
}

using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.UnitVariability;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteDrillDownSection : SummarySection {

        public bool IsUnitVariability { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsCumulative { get; set; }
        public double PercentageForDrilldown { get; set; }
        public double PercentileValue { get; set; }
        public string ReferenceCompoundName { get; set; }
        public List<DietaryAcuteDrillDownRecord> DrillDownSummaryRecords { get; set; }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceCompound,
            IDictionary<Food, FoodUnitVariabilityInfo> unitVariabilityDictionary,
            bool isProcessing,
            bool isUnitVariability,
            bool isCumulative,
            double percentageForDrilldown,
            bool isPerPerson
        ) {
            relativePotencyFactors = relativePotencyFactors ?? activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? activeSubstances.ToDictionary(r => r, r => 1D);

            PercentageForDrilldown = percentageForDrilldown;
            IsUnitVariability = isUnitVariability;
            IsProcessing = isProcessing;
            ReferenceCompoundName = referenceCompound.Name;
            IsCumulative = isCumulative;

            var selectedSimulatedIndividualDayIds = getDrillDownSimulatedIndividualDayIds(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, PercentageForDrilldown, isPerPerson);
            var drillDownTargets = dietaryIndividualDayIntakes
                .OrderBy(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Where(c => selectedSimulatedIndividualDayIds.Contains(c.SimulatedIndividualDayId))
                .ToList();

            var ix = BMath.Floor(drillDownTargets.Count() / 2);

            PercentileValue = drillDownTargets.ElementAt(ix).TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson);
            DrillDownSummaryRecords = new List<DietaryAcuteDrillDownRecord>();

            foreach (var item in drillDownTargets) {
                var bodyWeight = item.Individual.BodyWeight;

                var intakeSummaryPerFoodAsMeasuredRecords = item.IntakesPerFood
                    .GroupBy(ipf => ipf.FoodAsMeasured)
                    .Select(g => {
                        var detailedIntakesPerFood = g.Where(ipf => ipf is IntakePerFood);
                        var aggregatedIntakesPerFood = g.Where(ipf => ipf is AggregateIntakePerFood).Cast<AggregateIntakePerFood>();
                        var netAmountFoodAsMeasured = detailedIntakesPerFood.Where(ipf => ipf.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0).Sum(ipf => ipf.Amount)
                            + aggregatedIntakesPerFood.Sum(ipf => ipf.NetAmount);
                        var grossAmountFoodAsMeasured = g.Sum(ipf => ipf.Amount);
                        return new DietaryIntakeSummaryPerFoodRecord() {
                            FoodCode = g.Key.Code,
                            FoodName = g.Key.Name,
                            IntakePerMassUnit = g.Sum(ipf => ipf.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                            Concentration = g.Sum(ipf => ipf.Intake(relativePotencyFactors, membershipProbabilities)) / netAmountFoodAsMeasured,
                            AmountConsumed = netAmountFoodAsMeasured,
                            GrossAmountConsumed = grossAmountFoodAsMeasured,
                        };
                    })
                    .OrderBy(c => c.IntakePerMassUnit)
                    .ToList();

                var intakeSummaryPerFoodAsEatenRecords = item.DetailedIntakesPerFood
                    .GroupBy(ipf => ipf.FoodConsumption.Food)
                    .Select(g => {
                        var netAmountFoodAsEaten = g.Where(ipf => ipf.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) > 0)
                            .GroupBy(c => c.FoodConsumption)
                            .Sum(fc => fc.Average(c => c.Amount));

                        var grossAmountFoodAsEaten = g.GroupBy(c => c.FoodConsumption)
                            .Sum(fc => fc.Average(c => c.Amount));

                        return new DietaryIntakeSummaryPerFoodRecord() {
                            FoodCode = g.Key.Code,
                            FoodName = g.Key.Name,
                            IntakePerMassUnit = g.Sum(ipf => ipf.IntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)),
                            Concentration = g.Sum(ipf => ipf.Intake(relativePotencyFactors, membershipProbabilities)) / netAmountFoodAsEaten,
                            AmountConsumed = netAmountFoodAsEaten,
                            GrossAmountConsumed = grossAmountFoodAsEaten,
                        };
                    })
                    .OrderBy(c => c.IntakePerMassUnit)
                    .ToList();

                var aggregateIntakeSummaryPerCompoundRecords = item.IntakesPerFood
                        .SelectMany(ipf => ipf.IntakesPerCompound)
                        .Where(c => c.Exposure > 0)
                        .GroupBy(ipc => ipc.Compound)
                        .Select(g => new DietaryIntakeSummaryPerCompoundRecord {
                            CompoundCode = g.Key.Code,
                            CompoundName = g.Key.Name,
                            DietaryIntakeAmountPerBodyWeight = g.Sum(ipc => ipc.Intake(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound])) / bodyWeight,
                            RelativePotencyFactor = relativePotencyFactors[g.Key],
                        })
                    .ToList();

                var drillDownDictionary = unitVariabilityDictionary;
                var acuteIntakePerFoodRecords = item.DetailedIntakesPerFood
                    .Select(ipf => new DietaryAcuteIntakePerFoodRecord() {
                        FoodAsMeasuredCode = ipf.FoodAsMeasured.Code,
                        FoodAsMeasuredName = ipf.FoodAsMeasured.Name,
                        FoodAsEatenCode = ipf.FoodConsumption.Food.Code,
                        FoodAsEatenName = ipf.FoodConsumption.Food.Name,
                        FoodAsEatenAmount = ipf.FoodConsumption.Amount,
                        Translation = ipf.Amount / ipf.FoodConsumption.Amount,
                        AcuteIntakePerCompoundRecords = ipf.DetailedIntakesPerCompound
                            .Where(c => c.Exposure > 0)
                            .Select(ipc => {
                                var unitWeight = double.NaN;
                                var variabilityFactor = 1D;
                                var coefficientOfVariation = 0D;
                                var unitsInCompositeSample = 1;
                                var bulkingBlending = string.Empty;
                                var processingTypeDescription = string.Empty;
                                var hasProcessingType = ipc.ProcessingType != null;
                                if (IsUnitVariability) {
                                    if (drillDownDictionary.ContainsKey(ipf.FoodAsMeasured)) {
                                        var processingType = ipc.ProcessingType;
                                        var unitVariabilityRecord = drillDownDictionary[ipf.FoodAsMeasured].GetUnitVariabilityRecord(ipc.Compound, processingType);
                                        if (unitVariabilityRecord != null) {
                                            unitWeight = unitVariabilityRecord.UnitWeight;
                                            variabilityFactor = unitVariabilityRecord.UnitVariabilityFactor;
                                            unitsInCompositeSample = unitVariabilityRecord.UnitsInCompositeSample;
                                            coefficientOfVariation = unitVariabilityRecord.CoefficientOfVariation;
                                        }
                                    }
                                    if (hasProcessingType && ipc.ProcessingType.IsBulkingBlending) {
                                        bulkingBlending = "bulking/blending";
                                    }
                                }

                                if (IsProcessing && hasProcessingType) {
                                    processingTypeDescription = ipc.ProcessingType.Description;
                                }

                                return new DietaryAcuteIntakePerCompoundRecord() {
                                    CompoundCode = ipc.Compound.Code,
                                    CompoundName = ipc.Compound.Name,
                                    ProcessingFactor = ipc.ProcessingFactor,
                                    ProportionProcessing = ipc.ProcessingCorrectionFactor,
                                    Concentration = ipc.IntakePortion.Concentration,
                                    UnitVariabilityPortions = ipc.UnitIntakePortions ?? new List<IntakePortion>() { ipc.IntakePortion },
                                    UnitWeight = double.IsNaN(unitWeight) ? "-" : unitWeight.ToString("N0"),
                                    UnitVariabilityFactor = (hasProcessingType && ipc.ProcessingType.IsBulkingBlending) ? "1" : variabilityFactor.ToString("F2"),
                                    CoefficientOfVariation = (hasProcessingType && ipc.ProcessingType.IsBulkingBlending) ? "1" : coefficientOfVariation.ToString("F2"),
                                    UnitsInCompositeSample = (hasProcessingType && ipc.ProcessingType.IsBulkingBlending) ? "1" : unitsInCompositeSample.ToString("N0"),
                                    ProcessingTypeDescription = hasProcessingType ? $"{processingTypeDescription} {bulkingBlending}" : bulkingBlending,
                                    Rpf = relativePotencyFactors[ipc.Compound],
                                };
                            })
                            .OrderBy(ipcr => ipcr.Concentration)
                            .ToList(),
                    })
                    .OrderBy(ipfr => ipfr.FoodAsEatenName, System.StringComparer.OrdinalIgnoreCase)
                    .ThenBy(ipfr => ipfr.FoodAsMeasuredName, System.StringComparer.OrdinalIgnoreCase)
                    .ToList();

                List<DietaryOthersAcuteIntakePerFoodRecord> othersAcuteIntakePerFoodrecords = null;
                if (item.AggregateIntakesPerFood.Any()) {
                    othersAcuteIntakePerFoodrecords = item.AggregateIntakesPerFood
                        .Select(ipf => new DietaryOthersAcuteIntakePerFoodRecord() {
                            FoodAsMeasuredName = ipf.FoodAsMeasured.Name,
                            OthersAcuteIntakePerCompoundRecords = ipf.IntakesPerCompound
                                .Where(c => c.Exposure > 0)
                                .Select(ipc => {
                                    return new DietaryOthersAcuteIntakePerCompoundRecord() {
                                        CompoundName = ipc.Compound.Name,
                                        Intake = ipc.Intake(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]) / bodyWeight,
                                    };
                                })
                            .ToList()
                        })
                       .OrderBy(ipfr => ipfr.FoodAsMeasuredName, System.StringComparer.OrdinalIgnoreCase)
                       .ToList();

                    var othersTotalIntake = othersAcuteIntakePerFoodrecords.Select(c => c.OthersAcuteIntakePerCompoundRecords.Sum(i => i.Intake)).Sum();

                    if (othersTotalIntake > 0) {
                        acuteIntakePerFoodRecords.Add(new DietaryAcuteIntakePerFoodRecord() {
                            FoodAsMeasuredCode = "Others",
                            FoodAsMeasuredName = "Others",
                            FoodAsEatenCode = "Others",
                            FoodAsEatenName = "Others",
                            FoodAsEatenAmount = double.NaN,
                            Translation = double.NaN,
                            AcuteIntakePerCompoundRecords = new List<DietaryAcuteIntakePerCompoundRecord>() {
                                new DietaryAcuteIntakePerCompoundRecord(){
                                    CompoundCode = "Others",
                                    CompoundName = "Others",
                                    ProcessingFactor = double.NaN,
                                    ProportionProcessing = double.NaN,
                                    Concentration = double.NaN,
                                    UnitVariabilityPortions = new List<IntakePortion>() { new IntakePortion() { Amount = float.NaN, Concentration = float.NaN } },
                                    UnitWeight = "-",
                                    UnitVariabilityFactor = "-",
                                    CoefficientOfVariation = "-",
                                    UnitsInCompositeSample = "-",
                                    ProcessingTypeDescription = "-",
                                    Rpf = 1,
                                    Intake = othersTotalIntake,
                                },
                            }
                        });
                        intakeSummaryPerFoodAsEatenRecords.Add(
                            new DietaryIntakeSummaryPerFoodRecord() {
                                FoodCode = "Others",
                                FoodName = "Others",
                                IntakePerMassUnit = othersTotalIntake,
                                Concentration = double.NaN,
                                AmountConsumed = double.NaN,
                                GrossAmountConsumed = double.NaN,
                            });
                    }
                }
                var dietaryIntakePerBodyWeight = item.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson);

                var result = new DietaryAcuteDrillDownRecord() {
                    Guid = item.SimulatedIndividualDayId.ToString(),
                    IndividualCode = item.Individual.Code,
                    BodyWeight = bodyWeight,
                    SamplingWeight = item.Individual.SamplingWeight, // Indeed, here we print the original sampling weight!!!
                    Day = item.Day,
                    DietaryIntakePerMassUnit = dietaryIntakePerBodyWeight,
                    OthersIntakePerMassUnit = item.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities) / item.Individual.BodyWeight,
                    DietaryAbsorptionFactor = 1,
                    IntakeSummaryPerFoodAsMeasuredRecords = intakeSummaryPerFoodAsMeasuredRecords,
                    IntakeSummaryPerFoodAsEatenRecords = intakeSummaryPerFoodAsEatenRecords,
                    IntakeSummaryPerCompoundRecords = aggregateIntakeSummaryPerCompoundRecords,
                    AcuteIntakePerFoodRecords = acuteIntakePerFoodRecords,
                    //OthersAcuteIntakePerFoodRecords = othersAcuteIntakePerFoodrecords,
                };

                DrillDownSummaryRecords.Add(result);
            }
        }

        private static List<int> getDrillDownSimulatedIndividualDayIds(
                ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
                IDictionary<Compound, double> relativePotencyFactors,
                IDictionary<Compound, double> membershipProbabilities,
                double percentageForDrilldown,
                bool isPerPerson
            ) {
            var referenceIndividualIndex = BMath.Floor(dietaryIndividualDayIntakes.Count() * percentageForDrilldown / 100);

            var intakes = dietaryIndividualDayIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = dietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, percentageForDrilldown);
            referenceIndividualIndex = dietaryIndividualDayIntakes
                .Where(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) < weightedPercentileValue)
                .Count();

            var specifiedTakeNumer = 9;
            var lowerExtremePerson = specifiedTakeNumer - 1;
            if (percentageForDrilldown != 100) {
                lowerExtremePerson = BMath.Floor(specifiedTakeNumer / 2);
            }

            var ids = dietaryIndividualDayIntakes
                .OrderBy(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(specifiedTakeNumer)
                .Select(c => c.SimulatedIndividualDayId)
                .ToList();

            return ids;
        }
    }
}

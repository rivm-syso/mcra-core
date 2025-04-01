using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteDrillDownSection : SummarySection {

        public bool IsUnitVariability { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsCumulative { get; set; }
        public double VariabilityDrilldownPercentage { get; set; }
        public double PercentileValue { get; set; }
        public string ReferenceCompoundName { get; set; }

        public List<OverallIndividualDayDrillDownRecord> OverallIndividualDayDrillDownRecords { get; set; } = [];

        public void Summarize(
            SectionHeader header,
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

            VariabilityDrilldownPercentage = percentageForDrilldown;
            IsUnitVariability = isUnitVariability;
            IsProcessing = isProcessing;
            ReferenceCompoundName = referenceCompound.Name;
            IsCumulative = isCumulative;

            var selectedSimulatedIndividualDayIds = getDrillDownSimulatedIndividualDayIds(
                dietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                VariabilityDrilldownPercentage,
                isPerPerson
            );

            var drillDownTargets = dietaryIndividualDayIntakes
                .OrderBy(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .Where(c => selectedSimulatedIndividualDayIds.Contains(c.SimulatedIndividualDayId))
                .ToList();

            var ix = BMath.Floor(drillDownTargets.Count / 2);

            PercentileValue = drillDownTargets.ElementAt(ix)
                .TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson);
            var drilldownIndex = 0;
            foreach (var item in drillDownTargets) {
                drilldownIndex++;

                var bodyWeight = item.SimulatedIndividual.BodyWeight;
                var bwCorrectionFactor = isPerPerson ? 1 : item.SimulatedIndividual.BodyWeight;
                var intakeSummaryPerFoodAsEatenRecords = item.DetailedIntakesPerFood
                    .GroupBy(ipf => ipf.FoodConsumption.Food)
                    .Select(g => {
                        var netAmountFoodAsEaten = g
                            .Where(ipf => ipf.Intake(relativePotencyFactors, membershipProbabilities) > 0)
                            .GroupBy(c => c.FoodConsumption)
                            .Sum(fc => fc.Average(c => c.Amount));

                        var grossAmountFoodAsEaten = g.GroupBy(c => c.FoodConsumption)
                            .Sum(fc => fc.Average(c => c.Amount));

                        return new DietaryIntakeSummaryPerFoodRecord() {
                            FoodCode = g.Key.Code,
                            FoodName = g.Key.Name,
                            IntakePerMassUnit = g.Sum(ipf => ipf
                                .Intake(relativePotencyFactors, membershipProbabilities)) / bwCorrectionFactor,
                            Concentration = g.Sum(ipf => ipf
                                .Intake(relativePotencyFactors, membershipProbabilities)) / netAmountFoodAsEaten,
                            AmountConsumed = netAmountFoodAsEaten,
                            GrossAmountConsumed = grossAmountFoodAsEaten,
                        };
                    })
                    .OrderBy(c => c.IntakePerMassUnit)
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
                            .Where(c => c.Amount > 0)
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
                                        var unitVariabilityRecord = drillDownDictionary[ipf.FoodAsMeasured]
                                            .GetUnitVariabilityRecord(ipc.Compound, processingType);
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
                                    UnitVariabilityPortions = ipc.UnitIntakePortions ?? [ipc.IntakePortion],
                                    UnitWeight = unitWeight,
                                    UnitVariabilityFactor = (hasProcessingType
                                        && ipc.ProcessingType.IsBulkingBlending) ? 1 : variabilityFactor,
                                    CoefficientOfVariation = (hasProcessingType
                                        && ipc.ProcessingType.IsBulkingBlending) ? 1 : coefficientOfVariation,
                                    UnitsInCompositeSample = (hasProcessingType
                                        && ipc.ProcessingType.IsBulkingBlending) ? 1 : unitsInCompositeSample,
                                    ProcessingTypeDescription = hasProcessingType
                                        ? $"{processingTypeDescription} {bulkingBlending}"
                                        : bulkingBlending,
                                    Rpf = relativePotencyFactors[ipc.Compound],
                                };
                            })
                            .OrderBy(ipcr => ipcr.Concentration)
                            .ToList(),
                    })
                    .OrderBy(ipfr => ipfr.FoodAsEatenName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(ipfr => ipfr.FoodAsEatenCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(ipfr => ipfr.FoodAsMeasuredName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(ipfr => ipfr.FoodAsMeasuredCode, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                List<DietaryOthersAcuteIntakePerFoodRecord> othersAcuteIntakePerFoodrecords = null;
                if (item.AggregateIntakesPerFood.Any()) {
                    othersAcuteIntakePerFoodrecords = item.AggregateIntakesPerFood
                        .Select(ipf => new DietaryOthersAcuteIntakePerFoodRecord() {
                            FoodAsMeasuredName = ipf.FoodAsMeasured.Name,
                            OthersAcuteIntakePerCompoundRecords = ipf.IntakesPerCompound
                                .Where(c => c.Amount > 0)
                                .Select(ipc => {
                                    return new DietaryOthersAcuteIntakePerCompoundRecord() {
                                        CompoundName = ipc.Compound.Name,
                                        Intake = ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound],
                                            membershipProbabilities[ipc.Compound]) / bodyWeight,
                                    };
                                })
                            .ToList()
                        })
                       .OrderBy(ipfr => ipfr.FoodAsMeasuredName, StringComparer.OrdinalIgnoreCase)
                       .ToList();

                    var othersTotalIntake = othersAcuteIntakePerFoodrecords
                        .Select(c => c.OthersAcuteIntakePerCompoundRecords.Sum(i => i.Intake))
                        .Sum();

                    if (othersTotalIntake > 0) {
                        acuteIntakePerFoodRecords.Add(new DietaryAcuteIntakePerFoodRecord() {
                            FoodAsMeasuredCode = "Others",
                            FoodAsMeasuredName = "Others",
                            FoodAsEatenCode = "Others",
                            FoodAsEatenName = "Others",
                            FoodAsEatenAmount = double.NaN,
                            Translation = double.NaN,
                            AcuteIntakePerCompoundRecords = [
                                new DietaryAcuteIntakePerCompoundRecord(){
                                    CompoundCode = "Others",
                                    CompoundName = "Others",
                                    ProcessingFactor = double.NaN,
                                    ProportionProcessing = double.NaN,
                                    Concentration = double.NaN,
                                    UnitVariabilityPortions =
                                        [new IntakePortion() { Amount = float.NaN, Concentration = float.NaN }],
                                    UnitWeight = double.NaN,
                                    UnitVariabilityFactor = double.NaN,
                                    CoefficientOfVariation = double.NaN,
                                    UnitsInCompositeSample = double.NaN,
                                    ProcessingTypeDescription = "-",
                                    Rpf = 1,
                                    Intake = othersTotalIntake,
                                },
                            ]
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
                var dietaryIntakePerBodyWeight = item
                    .TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson);

                //Overall drilldown
                var overallIndividualDrilldownRecord = new OverallIndividualDayDrillDownRecord() {
                    SimulatedIndividualDayId = item.SimulatedIndividualDayId,
                    IndividualId = item.SimulatedIndividual.Code,
                    BodyWeight = bodyWeight,
                    Day = item.Day,
                    DietaryExposure = dietaryIntakePerBodyWeight,
                    SamplingWeight = item.SimulatedIndividual.SamplingWeight
                };
                OverallIndividualDayDrillDownRecords.Add(overallIndividualDrilldownRecord);


                //Substance drilldown
                var aggregateIntakeSummaryPerCompoundRecords = item.IntakesPerFood
                    .SelectMany(ipf => ipf.IntakesPerCompound)
                    .Where(c => c.Amount > 0)
                    .GroupBy(ipc => ipc.Compound)
                    .Select(g => new DietaryIntakeSummaryPerCompoundRecord {
                        CompoundCode = g.Key.Code,
                        CompoundName = g.Key.Name,
                        DietaryIntakeAmountPerBodyWeight = g.Sum(ipc => ipc
                            .EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]))
                            / bodyWeight,
                        RelativePotencyFactor = relativePotencyFactors[g.Key],
                    });

                //Modelled food drilldown
                var intakeSummaryPerFoodAsMeasuredRecords = item.IntakesPerFood
                    .GroupBy(ipf => ipf.FoodAsMeasured)
                    .Select(g => {
                        var detailedIntakesPerFood = g.Where(ipf => ipf is IntakePerFood);
                        var aggregatedIntakesPerFood = g.Where(ipf => ipf is AggregateIntakePerFood).Cast<AggregateIntakePerFood>();
                        var netAmountFoodAsMeasured = detailedIntakesPerFood
                            .Where(ipf => ipf.Intake(relativePotencyFactors, membershipProbabilities) > 0)
                            .Sum(ipf => ipf.Amount)
                            + aggregatedIntakesPerFood.Sum(ipf => ipf.NetAmount);
                        var grossAmountFoodAsMeasured = g.Sum(ipf => ipf.Amount);
                        return new DietaryIntakeSummaryPerFoodRecord() {
                            FoodCode = g.Key.Code,
                            FoodName = g.Key.Name,
                            IntakePerMassUnit = g.Sum(ipf => ipf.Intake(relativePotencyFactors, membershipProbabilities)
                                / bwCorrectionFactor),
                            Concentration = g.Sum(ipf => ipf.Intake(relativePotencyFactors, membershipProbabilities))
                                / netAmountFoodAsMeasured,
                            AmountConsumed = netAmountFoodAsMeasured,
                            GrossAmountConsumed = grossAmountFoodAsMeasured,
                        };
                    })
                    .OrderBy(c => c.IntakePerMassUnit);

                //Add subsections per item
                var idvSubSection = new AcuteIndividualDayDrilldownSection();
                var idvSubHeader = header.AddEmptySubSectionHeader($"Drilldown {drilldownIndex}", drilldownIndex);
                idvSubSection.Summarize(
                    idvSubHeader,
                    drilldownIndex,
                    overallIndividualDrilldownRecord,
                    acuteIntakePerFoodRecords,
                    aggregateIntakeSummaryPerCompoundRecords,
                    intakeSummaryPerFoodAsMeasuredRecords,
                    intakeSummaryPerFoodAsEatenRecords,
                    IsCumulative,
                    IsProcessing,
                    IsUnitVariability,
                    referenceCompound.Name,
                    dietaryIntakePerBodyWeight
                );
            }
        }

        private static HashSet<int> getDrillDownSimulatedIndividualDayIds(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForDrilldown,
            bool isPerPerson
        ) {
            var referenceIndividualIndex = BMath.Floor(dietaryIndividualDayIntakes.Count * percentageForDrilldown / 100);

            var intakes = dietaryIndividualDayIntakes
                .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
            var weights = dietaryIndividualDayIntakes
                .Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
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
                .ToHashSet();

            return ids;
        }
    }
}

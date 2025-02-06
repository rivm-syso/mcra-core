using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicDrillDownSection : SummarySection {
        public double VariabilityDrilldownPercentage { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsOIM { get; set; }
        public bool IsCumulative { get; set; }
        public string ReferenceCompoundName { get; set; }
        public string CofactorName { get; set; }
        public string CovariableName { get; set; }
        public double PercentileValue { get; set; }

        public List<OverallIndividualDrillDownRecord> OverallIndividualDrillDownRecords { get; set; } = [];
        public Dictionary<int, List<DetailedIndividualDrillDownRecord>> DetailedIndividualDrillDownRecords { get; set; } = [];
        public Dictionary<int, List<IndividualSubstanceDrillDownRecord>> IndividualSubstanceDrillDownRecords { get; set; } = [];
        public Dictionary<int, List<IndividualFoodDrillDownRecord>> IndividualModelledFoodDrillDownRecords { get; set; } = [];
        public Dictionary<int, List<IndividualFoodDrillDownRecord>> IndividualFoodAsEatenDrillDownRecords { get; set; } = [];
        /// <summary>
        /// OIM drilldown
        /// </summary>
        /// <param name="observedIndividualMeans"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="cofactor"></param>
        /// <param name="covariable"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="reference"></param>
        /// <param name="processingFactorModel"></param>
        /// <param name="isCumulative"></param>
        /// <param name="percentageForDrilldown"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IndividualProperty cofactor,
            IndividualProperty covariable,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound reference,
            bool isProcessing,
            bool isCumulative,
            double percentageForDrilldown,
            bool isPerPerson
        ) {
            if (observedIndividualMeans == null) {
                return;
            }
            relativePotencyFactors = relativePotencyFactors ?? activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? activeSubstances.ToDictionary(r => r, r => 1D);

            setViewProperties(cofactor, covariable, reference.Name, isProcessing, isCumulative, percentageForDrilldown);
            IsOIM = true;

            var selectedIndividualIds = getOimDrillDownIndividualIds(
                percentageForDrilldown,
                observedIndividualMeans
            );

            var drillDownTargets = observedIndividualMeans
                .OrderBy(c => c.DietaryIntakePerMassUnit)
                .Where(c => selectedIndividualIds.Contains(c.SimulatedIndividualId))
                .ToArray();

            var ix = BMath.Floor(drillDownTargets.Length / 2);
            PercentileValue = drillDownTargets[ix].DietaryIntakePerMassUnit;

            foreach (var item in drillDownTargets) {
                var individualDayIntakes = dietaryIndividualDayIntakes
                    .Where(r => r.SimulatedIndividualId == item.SimulatedIndividualId)
                    .ToList();
                var bodyWeight = item.Individual.BodyWeight;
                var dietaryIntakePerBodyWeight = individualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
                var othersDietaryIntakePerMassUnit = individualDayIntakes.Sum(c => c.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities)) / individualDayIntakes.First().Individual.BodyWeight;
                var dayDrillDownRecords = getDayDrillDownRecord(individualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);

                //Overall drilldown
                var overallIndividualDrilldownRecord = new OverallIndividualDrillDownRecord() {
                    SimulatedIndividualId = item.SimulatedIndividualId,
                    IndividualId = individualDayIntakes.First().Individual.Code,
                    BodyWeight = individualDayIntakes.First().Individual.BodyWeight,
                    Cofactor = item.Individual.Cofactor ?? string.Empty,
                    Covariable = item.Individual.Covariable,
                    SamplingWeight = individualDayIntakes.First().Individual.SamplingWeight,
                    ObservedIndividualMean = dietaryIntakePerBodyWeight,
                    NumberOfSurveyDays = dayDrillDownRecords.Count,
                    PositiveSurveyDays = item.NumberOfDays,
                };
                OverallIndividualDrillDownRecords.Add(overallIndividualDrilldownRecord);

                //Detailed individual drilldown
                var detailedIndividualDrilldownRecords = new List<DetailedIndividualDrillDownRecord>();
                var showRpf = dayDrillDownRecords.Any(r => r.ChronicIntakePerFoodRecords
                    .Any(ipf => ipf.ChronicIntakePerCompoundRecords
                        .Any(ipc => !double.IsNaN(ipc.Rpf) && ipc.Rpf != 1D)));

                foreach (var dayDrillDown in dayDrillDownRecords) {
                    var numberOfDays = dayDrillDownRecords.Count;
                    var detailedIntakePerFoodRecord = dayDrillDown.ChronicIntakePerFoodRecords;
                    foreach (var ipf in detailedIntakePerFoodRecord) {
                        foreach (var ipc in ipf.ChronicIntakePerCompoundRecords) {
                            if (ipc.Concentration > 0 || double.IsNaN(ipc.Concentration)) {
                                var exposure = ipf.FoodAsMeasuredAmount * ipc.Concentration * ipc.ProcessingFactor / item.Individual.BodyWeight / ipc.ProportionProcessing;
                                var detailedIndividualDrilldownRecord = new DetailedIndividualDrillDownRecord() {
                                    Day = dayDrillDown.Day,
                                    FoodAsEaten = ipf.FoodAsEatenName,
                                    Amount = ipf.FoodAsEatenAmount,
                                    ModelledFood = ipf.FoodAsMeasuredName,
                                    ConversionFactor = ipf.Translation,
                                    PortionAmount = ipf.FoodAsMeasuredAmount,
                                    Substance = ipc.CompoundName,
                                    ConcentrationInSample = ipc.Concentration,
                                    ProcessingFactor = ipc.ProcessingFactor,
                                    ProcessingCorrectionFactor = ipc.ProportionProcessing,
                                    Exposure = double.IsNaN(ipf.FoodAsMeasuredAmount)
                                        ? ipc.Intake
                                        : exposure,
                                    Rpf = ipc.Rpf,
                                    EquivalentExposure = showRpf && !double.IsNaN(ipf.FoodAsMeasuredAmount)
                                        ? ipc.Rpf * exposure
                                        : (showRpf ? ipc.Intake : double.NaN),
                                    Percentage = showRpf && !double.IsNaN(ipf.FoodAsMeasuredAmount)
                                        ? ipc.Rpf * exposure / item.DietaryIntakePerMassUnit / numberOfDays
                                        : (showRpf ? ipc.Intake / item.DietaryIntakePerMassUnit / numberOfDays : double.NaN)
                                };
                                detailedIndividualDrilldownRecords.Add(detailedIndividualDrilldownRecord);
                            }
                        }
                    }
                }
                DetailedIndividualDrillDownRecords.Add(item.SimulatedIndividualId, detailedIndividualDrilldownRecords);

                getSubstanceDrillDown(item.SimulatedIndividualId, bodyWeight, dayDrillDownRecords);

                getModelledFoodsDrillDown(item.SimulatedIndividualId, dayDrillDownRecords);

                getFoodsAsEatenDrillDown(item.SimulatedIndividualId, dayDrillDownRecords);
            }
        }

        private void getFoodsAsEatenDrillDown(int simulatedIndividualId, List<DietaryDayDrillDownRecord> dayDrillDownRecords) {
            //Food as eaten drilldown
            var individualFoodAsEatenDrilldownRecords = new List<IndividualFoodDrillDownRecord>();
            foreach (var dayDrillDown in dayDrillDownRecords) {
                var records = dayDrillDown.IntakeSummaryPerFoodAsEatenRecords;
                foreach (var ipf in records) {
                    var individualFoodAsEatenDrilldownRecord = new IndividualFoodDrillDownRecord() {
                        Day = dayDrillDown.Day,
                        FoodName = ipf.FoodName,
                        FoodCode = ipf.FoodCode,
                        TotalConsumption = ipf.GrossAmountConsumed,
                        NetConsumption = ipf.AmountConsumed,
                        EquivalentExposure = ipf.Concentration,
                        Exposure = ipf.IntakePerMassUnit,
                    };
                    individualFoodAsEatenDrilldownRecords.Add(individualFoodAsEatenDrilldownRecord);
                }
            }
            IndividualFoodAsEatenDrillDownRecords.Add(simulatedIndividualId, individualFoodAsEatenDrilldownRecords);
        }

        private void getModelledFoodsDrillDown(int simulatedIndividualId, List<DietaryDayDrillDownRecord> dayDrillDownRecords) {
            //Modelled food drilldown
            var individualModelledFoodDrilldownRecords = new List<IndividualFoodDrillDownRecord>();
            foreach (var dayDrillDown in dayDrillDownRecords) {
                var records = dayDrillDown.IntakeSummaryPerFoodAsMeasuredRecords;
                foreach (var record in records) {
                    var individualModellledFoodDrilldownRecord = new IndividualFoodDrillDownRecord() {
                        Day = dayDrillDown.Day,
                        FoodName = record.FoodName,
                        FoodCode = record.FoodCode,
                        TotalConsumption = record.GrossAmountConsumed,
                        NetConsumption = record.AmountConsumed,
                        EquivalentExposure = record.Concentration,
                        Exposure = record.IntakePerMassUnit,
                    };
                    individualModelledFoodDrilldownRecords.Add(individualModellledFoodDrilldownRecord);
                }
            }
            IndividualModelledFoodDrillDownRecords.Add(simulatedIndividualId, individualModelledFoodDrilldownRecords);
        }

        private void getSubstanceDrillDown(int simulatedIndividualId, double bodyWeight, List<DietaryDayDrillDownRecord> dayDrillDownRecords) {
            //Substance drilldown
            var individualSubstanceDrilldownRecords = new List<IndividualSubstanceDrillDownRecord>();
            foreach (var dayDrillDown in dayDrillDownRecords) {
                var records = dayDrillDown.DietaryIntakeSummaryPerCompoundRecords;
                foreach (var record in records) {
                    var individualSubstanceDrilldownRecord = new IndividualSubstanceDrillDownRecord() {
                        Day = dayDrillDown.Day,
                        SubstanceName = record.CompoundName,
                        SubstanceCode = record.CompoundCode,
                        ExposurePerDay = bodyWeight * record.DietaryIntakeAmountPerBodyWeight / record.RelativePotencyFactor,
                        Exposure = record.DietaryIntakeAmountPerBodyWeight / record.RelativePotencyFactor,
                        Rpf = record.RelativePotencyFactor,
                        EquivalentExposure = record.DietaryIntakeAmountPerBodyWeight
                    };
                    individualSubstanceDrilldownRecords.Add(individualSubstanceDrilldownRecord);
                }
            }
            IndividualSubstanceDrillDownRecords.Add(simulatedIndividualId, individualSubstanceDrilldownRecords);
        }

        private void setViewProperties(
            IndividualProperty cofactor,
            IndividualProperty covariable,
            string referenceCompoundName,
            bool isProcessing,
            bool isCumulative,
            double percentageForDrilldown
        ) {
            VariabilityDrilldownPercentage = percentageForDrilldown;
            ReferenceCompoundName = referenceCompoundName;
            IsProcessing = isProcessing;
            IsCumulative = isCumulative;
            CofactorName = cofactor?.Name;
            CovariableName = covariable?.Name;
        }

        /// <summary>
        /// For parametric models, LNN, BNN, LNN0.
        /// </summary>
        /// <param name="individualUsualIntakes"></param>
        /// <param name="observedIndividualMeans"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="cofactor"></param>
        /// <param name="covariable"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceCompound"></param>
        /// <param name="processingFactorModel"></param>
        /// <param name="isCumulative"></param>
        /// <param name="percentageForDrilldown"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            ICollection<ModelAssistedIntake> individualUsualIntakes,
            ICollection<DietaryIndividualIntake> observedIndividualMeans,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IndividualProperty cofactor,
            IndividualProperty covariable,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceCompound,
            bool isProcessing,
            bool isCumulative,
            double percentageForDrilldown,
            bool isPerPerson
        ) {
            relativePotencyFactors = relativePotencyFactors ?? activeSubstances.ToDictionary(r => r, r => 1D);
            membershipProbabilities = membershipProbabilities ?? activeSubstances.ToDictionary(r => r, r => 1D);

            setViewProperties(cofactor, covariable, referenceCompound.Name, isProcessing, isCumulative, percentageForDrilldown);

            //Controleer dit samplingweights idem voor oim
            var usualIntakeIndividual = individualUsualIntakes
                .Join(observedIndividualMeans,
                    f => f.SimulatedIndividualId,
                    a => a.SimulatedIndividualId,
                    (f, a) => {
                        var individualDayIntakes = dietaryIndividualDayIntakes
                            .Where(r => r.SimulatedIndividualId == a.SimulatedIndividualId)
                            .ToList();
                        return new {
                            UsualIntake = f.UsualIntake,
                            ObservedIndividualMean = a.DietaryIntakePerMassUnit,
                            SimulatedIndividualId = f.SimulatedIndividualId,
                            DietaryIndividualDayIntakes = individualDayIntakes,
                            FrequencyPrediction = f.FrequencyPrediction,
                            ModelAssistedFrequency = f.ModelAssistedPrediction,
                            ModelAssistedAmount = f.ModelAssistedAmount,
                            AmountPrediction = f.AmountPrediction,
                            ShrinkageFactor = f.ShrinkageFactor,
                            NDays = f.NDays,
                            TransformedOIM = f.TransformedOIM,
                            Cofactor = a.Individual.Cofactor ?? string.Empty,
                            Covariable = a.Individual.Covariable,
                            SamplingWeight = f.IndividualSamplingWeight
                        };
                    })
                .ToList();

            var selectedIndividuals = getUsualIntakeDrilldownIndividualIds(
                individualUsualIntakes,
                percentageForDrilldown
            );

            var drillDownTargets = usualIntakeIndividual
               .OrderBy(ui => ui.UsualIntake)
               .Where(c => selectedIndividuals.Contains(c.SimulatedIndividualId))
               .ToArray();

            var ix = BMath.Floor(drillDownTargets.Length / 2);
            PercentileValue = drillDownTargets[ix].ObservedIndividualMean;
            foreach (var item in drillDownTargets) {
                var idi = item.DietaryIndividualDayIntakes;
                var bodyWeight = idi.First().Individual.BodyWeight;
                var dietaryIntakePerBodyWeight = idi.Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
                var othersDietaryIntakePerMassUnit = idi.Sum(c => c.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities)) / idi.First().Individual.BodyWeight;
                var dayDrillDownRecords = getDayDrillDownRecord(idi, relativePotencyFactors, membershipProbabilities, isPerPerson);

                //Overall drilldown
                var overallIndividualDrilldownRecord = new OverallIndividualDrillDownRecord() {
                    SimulatedIndividualId = item.SimulatedIndividualId,
                    IndividualId = idi.First().Individual.Code,
                    BodyWeight = idi.First().Individual.BodyWeight,
                    Cofactor = item.Cofactor ?? string.Empty,
                    Covariable = item.Covariable,
                    SamplingWeight = item.SamplingWeight,
                    ObservedIndividualMean = item.ObservedIndividualMean,
                    NumberOfSurveyDays = dayDrillDownRecords.Count,
                    PositiveSurveyDays = item.NDays,
                    FrequencyPrediction = item.FrequencyPrediction,
                    AmountPrediction = item.AmountPrediction,
                    ShrinkageFactor = item.ShrinkageFactor,
                    ModelAssistedExposure = item.UsualIntake,
                    MeanTransformedIntake = item.TransformedOIM
                };
                OverallIndividualDrillDownRecords.Add(overallIndividualDrilldownRecord);

                //Detailed individual drilldown
                var detailedIndividualDrilldownRecords = new List<DetailedIndividualDrillDownRecord>();
                var showRpf = dayDrillDownRecords.Any(r => r.ChronicIntakePerFoodRecords
                    .Any(ipf => ipf.ChronicIntakePerCompoundRecords
                        .Any(ipc => !double.IsNaN(ipc.Rpf) && ipc.Rpf != 1D)));
                foreach (var dayDrillDown in dayDrillDownRecords) {
                    var numberOfDays = dayDrillDownRecords.Count;
                    var detailedIntakePerFoodRecord = dayDrillDown.ChronicIntakePerFoodRecords;
                    foreach (var ipf in detailedIntakePerFoodRecord) {
                        foreach (var ipc in ipf.ChronicIntakePerCompoundRecords) {
                            if (ipc.Concentration > 0 || double.IsNaN(ipc.Concentration)) {
                                var exposure = ipf.FoodAsMeasuredAmount * ipc.Concentration * ipc.ProcessingFactor / bodyWeight / ipc.ProportionProcessing;
                                var detailedIndividualDrilldownRecord = new DetailedIndividualDrillDownRecord() {
                                    Day = dayDrillDown.Day,
                                    FoodAsEaten = ipf.FoodAsEatenName,
                                    Amount = ipf.FoodAsEatenAmount,
                                    ModelledFood = ipf.FoodAsMeasuredName,
                                    ConversionFactor = ipf.Translation,
                                    PortionAmount = ipf.FoodAsMeasuredAmount,
                                    Substance = ipc.CompoundName,
                                    ConcentrationInSample = ipc.Concentration,
                                    ProcessingFactor = ipc.ProcessingFactor,
                                    ProcessingCorrectionFactor = ipc.ProportionProcessing,
                                    Exposure = double.IsNaN(ipf.FoodAsMeasuredAmount)
                                        ? ipc.Intake
                                        : exposure,
                                    Rpf = ipc.Rpf,
                                    EquivalentExposure = showRpf && !double.IsNaN(ipf.FoodAsMeasuredAmount)
                                        ? ipc.Rpf * exposure
                                        : (showRpf ? ipc.Intake : double.NaN),
                                    Percentage = showRpf && !double.IsNaN(ipf.FoodAsMeasuredAmount)
                                        ? ipc.Rpf * exposure / item.ObservedIndividualMean / numberOfDays
                                        : (showRpf ? ipc.Intake / item.ObservedIndividualMean / numberOfDays : double.NaN)
                                };
                                detailedIndividualDrilldownRecords.Add(detailedIndividualDrilldownRecord);
                            }
                        }
                    }
                }
                DetailedIndividualDrillDownRecords.Add(item.SimulatedIndividualId, detailedIndividualDrilldownRecords);

                getSubstanceDrillDown(item.SimulatedIndividualId, bodyWeight, dayDrillDownRecords);

                getModelledFoodsDrillDown(item.SimulatedIndividualId, dayDrillDownRecords);

                getFoodsAsEatenDrillDown(item.SimulatedIndividualId, dayDrillDownRecords);
            }
        }

        /// <summary>
        /// Gives aggregated and detailed info for each exposure day
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="isPerPerson"></param>
        /// <returns></returns>
        private static List<DietaryDayDrillDownRecord> getDayDrillDownRecord(
            List<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var dayDrillDownRecord = new List<DietaryDayDrillDownRecord>();
            foreach (var dietaryIndividualDayIntake in dietaryIndividualDayIntakes) {

                var intakesPerFood = dietaryIndividualDayIntake
                    .IntakesPerFood
                    .Where(ipf => ipf.Intake(relativePotencyFactors, membershipProbabilities) > 0)
                    .ToList();

                var intakeSummaryPerFoodAsMeasuredRecords = intakesPerFood
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

                var intakeSummaryPerFoodAsEatenRecords = intakesPerFood
                       .Where(ipf => ipf is IntakePerFood)
                       .Cast<IntakePerFood>()
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

                var chronicIntakePerFoodRecords = intakesPerFood
                   .Where(ipf => ipf is IntakePerFood)
                   .Cast<IntakePerFood>()
                   .Select(f => new DietaryChronicIntakePerFoodRecord() {
                       FoodAsMeasuredName = f.FoodAsMeasured.Name,
                       FoodAsMeasuredCode = f.FoodAsMeasured.Code,
                       FoodAsMeasuredAmount = f.Amount,
                       FoodAsEatenName = f.FoodConsumption.Food.Name,
                       FoodAsEatenCode = f.FoodConsumption.Food.Code,
                       FoodAsEatenAmount = f.FoodConsumption.Amount,
                       Translation = f.Amount / f.FoodConsumption.Amount,
                       ChronicIntakePerCompoundRecords = f.DetailedIntakesPerCompound
                           .Select(ipc => new DietaryChronicIntakePerCompoundRecord() {
                               CompoundCode = ipc.Compound.Code,
                               CompoundName = ipc.Compound.Name,
                               ProcessingFactor = ipc.ProcessingFactor,
                               ProportionProcessing = ipc.ProcessingCorrectionFactor,
                               Concentration = ipc.MeanConcentration,
                               Rpf = relativePotencyFactors[ipc.Compound],
                           })
                           .ToList(),
                   })
                   .ToList();

                var aggregateIntakeSummaryPerCompoundRecords = intakesPerFood
                        .SelectMany(ipf => ipf.IntakesPerCompound)
                        .Where(c => c.Amount > 0)
                        .GroupBy(ipc => ipc.Compound)
                        .Select(g => new DietaryIntakeSummaryPerCompoundRecord {
                            CompoundCode = g.Key.Code,
                            CompoundName = g.Key.Name,
                            DietaryIntakeAmountPerBodyWeight = g.Sum(ipc => ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound])) / dietaryIndividualDayIntake.Individual.BodyWeight,
                            RelativePotencyFactor = relativePotencyFactors[g.Key],
                        })
                 .ToList();

                List<DietaryOthersChronicIntakePerFoodRecord> othersChronicIntakePerFoodRecords = null;
                if (dietaryIndividualDayIntakes.Any(r => r.AggregateIntakesPerFood.Any())) {
                    othersChronicIntakePerFoodRecords = dietaryIndividualDayIntakes
                        .SelectMany(c => c.AggregateIntakesPerFood)
                        .Select(f => new DietaryOthersChronicIntakePerFoodRecord() {
                            FoodAsMeasuredName = f.FoodAsMeasured.Name,
                            OthersChronicIntakePerCompoundRecords = f.IntakesPerCompound
                                .Where(ipc => ipc.Amount > 0)
                                .Select(ipc => new DietaryOthersChronicIntakePerCompoundRecord() {
                                    CompoundName = ipc.Compound.Name,
                                    Intake = ipc.EquivalentSubstanceAmount(relativePotencyFactors[ipc.Compound], membershipProbabilities[ipc.Compound]) / dietaryIndividualDayIntake.Individual.BodyWeight
                                })
                            .ToList()
                        })
                        .ToList();

                    var othersTotalIntake = othersChronicIntakePerFoodRecords.Select(c => c.OthersChronicIntakePerCompoundRecords.Sum(i => i.Intake)).Sum();

                    if (othersTotalIntake > 0) {
                        chronicIntakePerFoodRecords.Add(new DietaryChronicIntakePerFoodRecord() {
                            FoodAsMeasuredName = "Others",
                            FoodAsMeasuredCode = "Others",
                            FoodAsMeasuredAmount = double.NaN,
                            FoodAsEatenName = "Others",
                            FoodAsEatenCode = "Others",
                            FoodAsEatenAmount = double.NaN,
                            Translation = double.NaN,
                            ChronicIntakePerCompoundRecords = [
                                new DietaryChronicIntakePerCompoundRecord(){
                                    CompoundCode = "Others",
                                    CompoundName = "Others",
                                    ProcessingFactor = double.NaN,
                                    ProportionProcessing = double.NaN,
                                    Concentration = double.NaN,
                                    Rpf = 1,
                                    Intake = othersTotalIntake,
                                    }
                                ]
                        });
                    }
                }

                dayDrillDownRecord.Add(new DietaryDayDrillDownRecord() {
                    Day = dietaryIndividualDayIntake.Day,
                    OthersDietaryIntakePerBodyWeight = dietaryIndividualDayIntake.TotalOtherIntakesPerCompound(relativePotencyFactors, membershipProbabilities),
                    TotalDietaryIntakePerBodyWeight = dietaryIndividualDayIntake.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson),
                    IntakeSummaryPerFoodAsMeasuredRecords = intakeSummaryPerFoodAsMeasuredRecords,
                    IntakeSummaryPerFoodAsEatenRecords = intakeSummaryPerFoodAsEatenRecords,
                    ChronicIntakePerFoodRecords = chronicIntakePerFoodRecords,
                    OthersChronicIntakePerFoodRecords = othersChronicIntakePerFoodRecords,
                    DietaryIntakeSummaryPerCompoundRecords = aggregateIntakeSummaryPerCompoundRecords,
                });
            }
            return dayDrillDownRecord;
        }

        private HashSet<int> getOimDrillDownIndividualIds(
            double percentageForDrilldown,
            ICollection<DietaryIndividualIntake> observedIndividualMeans
        ) {

            VariabilityDrilldownPercentage = percentageForDrilldown;
            var referenceIndividualIndex = BMath.Floor(observedIndividualMeans.Count * VariabilityDrilldownPercentage / 100);
            var intakes = observedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit);
            var weights = observedIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, VariabilityDrilldownPercentage);
            referenceIndividualIndex = observedIndividualMeans
                .Where(c => c.DietaryIntakePerMassUnit < weightedPercentileValue)
                .Count();

            var specifiedTakeNumer = 9;
            var lowerExtremePerson = specifiedTakeNumer - 1;
            if (VariabilityDrilldownPercentage != 100) {
                lowerExtremePerson = BMath.Floor(specifiedTakeNumer / 2);
            }

            var ids = observedIndividualMeans
                .OrderBy(c => c.DietaryIntakePerMassUnit)
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(specifiedTakeNumer)
                .Select(c => c.SimulatedIndividualId)
                .ToHashSet();

            return ids;
        }

        private HashSet<int> getUsualIntakeDrilldownIndividualIds(
            ICollection<ModelAssistedIntake> individualUsualIntakes,
            double percentageForDrilldown
        ) {
            VariabilityDrilldownPercentage = percentageForDrilldown;
            var referenceIndividualIndex = BMath.Floor(individualUsualIntakes.Count * VariabilityDrilldownPercentage / 100);
            var intakes = individualUsualIntakes.Select(c => c.UsualIntake);
            var weights = individualUsualIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var weightedPercentileValue = intakes.PercentilesWithSamplingWeights(weights, VariabilityDrilldownPercentage);
            referenceIndividualIndex = individualUsualIntakes
                .Where(c => c.UsualIntake < weightedPercentileValue)
                .Count();

            var specifiedTakeNumer = 9;
            var lowerExtremePerson = specifiedTakeNumer - 1;
            if (VariabilityDrilldownPercentage != 100) {
                lowerExtremePerson = BMath.Floor(specifiedTakeNumer / 2);
            }

            var ids = individualUsualIntakes
                .OrderBy(c => c.UsualIntake)
                .Skip(referenceIndividualIndex - lowerExtremePerson)
                .Take(specifiedTakeNumer)
                .Select(c => c.SimulatedIndividualId)
                .ToHashSet();

            return ids;
        }
    }
}

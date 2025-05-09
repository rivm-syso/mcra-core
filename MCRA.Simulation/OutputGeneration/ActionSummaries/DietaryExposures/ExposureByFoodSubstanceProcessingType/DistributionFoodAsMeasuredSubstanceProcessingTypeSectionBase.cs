﻿using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionFoodAsMeasuredSubstanceProcessingTypeSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<FoodAsMeasuredSubstanceProcessingTypeRecord> Records { get; set; }

        public int UncertaintyCycles { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double UncertaintyLowerBound { get; set; }
        public double UncertaintyUpperBound { get; set; }

        protected List<FoodAsMeasuredSubstanceProcessingTypeRecord> summarizeAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var intakesPerFoodsAsMeasured = dietaryIndividualDayIntakes
                .Where(c => c.IsPositiveIntake())
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(c => c.DetailedIntakesPerFood,
                    (idi, ipf) => (
                        ipf,
                        idi.SimulatedIndividual.SamplingWeight,
                        idi.SimulatedIndividual.BodyWeight,
                        idi.SimulatedIndividualDayId
                    ))
                .ToList();

            var intakesPerCompound = substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(substance => {
                    var compoundFoodCollection = intakesPerFoodsAsMeasured
                        .SelectMany(fam =>
                            fam.ipf.DetailedIntakesPerCompound.Where(s => s.Compound == substance),
                            (fam, ipc) => (
                                FoodAsMeasured: fam.ipf.FoodAsMeasured,
                                IntakePerCompound: ipc,
                                ProcessingType: ipc.ProcessingType,
                                Exposure: isPerPerson ? ipc.Amount : ipc.Amount / fam.BodyWeight,
                                Intake: isPerPerson ? ipc.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance]) : ipc.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance]) / fam.BodyWeight,
                                SamplingWeight: fam.SamplingWeight,
                                SimulatedIndividualDayId: fam.SimulatedIndividualDayId
                            ))
                        .ToList();
                    return compoundFoodCollection;
                })
                .ToList();

            var totalIntake = dietaryIndividualDayIntakes
                .Sum(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * c.SimulatedIndividual.SamplingWeight);
            var sumSamplingWeights = dietaryIndividualDayIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight);

            var results = intakesPerCompound
                .Where(c => c.Any())
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(c => {
                    var substance = c.First().IntakePerCompound.Compound;
                    var intakesPerFoodPerCompound = c
                        .GroupBy(gr => (
                            FoodAsMeasured: gr.FoodAsMeasured,
                            ProcessingTypeCode: gr.ProcessingType?.Code,
                            ProcessingName: gr.ProcessingType?.Name ?? "Others"
                        ))
                        .Select(g =>
                             new FoodAsMeasuredSubstanceProcessingTypeRecord {
                                 SubstanceCode = substance.Code,
                                 SubstanceName = substance.Name,
                                 FoodCode = g.Key.FoodAsMeasured.Code,
                                 FoodName = g.Key.FoodAsMeasured.Name,
                                 ProcessingTypeName = g.Key.ProcessingName,
                                 ProcessingTypeCode = g.Key.ProcessingTypeCode,
                                 ProcessingFactor = g.Average(ipc => ipc.IntakePerCompound.ProcessingFactor),
                                 ProcessingCorrectionFactor = g.AverageOrZero(ipc => ipc.IntakePerCompound.ProcessingCorrectionFactor),
                                 MeanAll = g.Sum(ipc => ipc.Exposure * ipc.SamplingWeight) / sumSamplingWeights,
                                 Contribution = g.Sum(ipc => ipc.Intake * ipc.SamplingWeight) / totalIntake,
                                 NumberOfPositives = g.Distinct(ipc => ipc.SimulatedIndividualDayId).Count(),
                                 MeanPositives = g.Sum(ipc => ipc.Exposure * ipc.SamplingWeight) / g.Sum(ipc => ipc.SamplingWeight),
                                 Contributions = [],
                             });
                    return intakesPerFoodPerCompound;
                })
                .ToList();

            return results;
        }

        protected List<FoodAsMeasuredSubstanceProcessingTypeRecord> summarizeChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            bool isPerPerson
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            var totalIntake = relativePotencyFactors != null
                ? dietaryIndividualDayIntakes
                    .GroupBy(c => c.SimulatedIndividual.Id)
                    .Sum(c => c.Sum(r => r.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * r.SimulatedIndividual.SamplingWeight) / c.Count())
                : double.NaN;
            var individualDayCountLookup = dietaryIndividualDayIntakes
               .GroupBy(c => c.SimulatedIndividual.Id)
               .ToDictionary(c => c.Key, c => c.Count());

            var sumSamplingWeights = dietaryIndividualDayIntakes
                .GroupBy(c => c.SimulatedIndividual.Id)
                .Sum(c => c.First().SimulatedIndividual.SamplingWeight);

            var intakesPerFoodsAsMeasured = dietaryIndividualDayIntakes
                .Where(c => c.IsPositiveIntake())
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(c => c.DetailedIntakesPerFood,
                    (idi, ipf) => (
                        ipf,
                        idi.SimulatedIndividual.SamplingWeight,
                        idi.SimulatedIndividual.Id,
                        idi.SimulatedIndividual
                    ))
                .ToList();

            var intakesPerCompound = substances
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(substance => {
                    var compoundFoodCollection = intakesPerFoodsAsMeasured
                        .SelectMany(fam => fam.ipf.DetailedIntakesPerCompound
                            .Where(s => s.Compound == substance),
                                (fam, ipc) => (
                                    FoodAsMeasured: fam.ipf.FoodAsMeasured,
                                    IntakePerCompound: ipc,
                                    ProcessingType: ipc.ProcessingType,
                                    Exposure: isPerPerson ? ipc.Amount : ipc.Amount / fam.SimulatedIndividual.BodyWeight,
                                    Intake: isPerPerson ? ipc.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance]) : ipc.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance]) / fam.SimulatedIndividual.BodyWeight,
                                    SamplingWeight: fam.SimulatedIndividual.SamplingWeight,
                                    BodyWeight: fam.SimulatedIndividual.BodyWeight,
                                    SimulatedIndividualId: fam.SimulatedIndividual.Id,
                                    NumberOfDaysInSurvey: individualDayCountLookup[fam.SimulatedIndividual.Id]
                                )
                            )
                            .ToList();
                    return compoundFoodCollection;
                })
                .ToList();

            return intakesPerCompound
                .Where(c => c.Any())
                .AsParallel()
                .WithCancellation(cancelToken)
                .SelectMany(c => {
                    var substance = c.First().IntakePerCompound.Compound;
                    var intakesPerFoodPerCompound = c.GroupBy(gr => (
                        FoodAsMeasured: gr.FoodAsMeasured,
                        ProcessingType: gr.ProcessingType,
                        SimulatedIndividualId: gr.SimulatedIndividualId
                    ))
                    .Select(g => (
                        FoodAsMeasured: g.Key.FoodAsMeasured,
                        ProcessingType: g.Key.ProcessingType,
                        ProcessingFactor: g.Average(ipc => ipc.IntakePerCompound.ProcessingFactor),
                        ProcessingCorrectionFactor: g.AverageOrZero(ipc => ipc.IntakePerCompound.ProcessingCorrectionFactor),
                        SamplingWeight: g.First().SamplingWeight,
                        Exposure: g.Sum(ipc => ipc.IntakePerCompound.Amount / (isPerPerson ? 1 : g.First().BodyWeight)) / g.First().NumberOfDaysInSurvey,
                        Intake: g.Sum(ipc => ipc.IntakePerCompound.EquivalentSubstanceAmount(relativePotencyFactors[substance], membershipProbabilities[substance]) / (isPerPerson ? 1 : g.First().BodyWeight)) / g.First().NumberOfDaysInSurvey,
                        SimulatedIndividualId: g.First().SimulatedIndividualId
                    ))
                    .GroupBy(gr => (
                        FoodAsMeasured: gr.FoodAsMeasured,
                        ProcessingType: gr.ProcessingType
                    ))
                    .Select(g => new FoodAsMeasuredSubstanceProcessingTypeRecord {
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        FoodCode = g.Key.FoodAsMeasured.Code,
                        FoodName = g.Key.FoodAsMeasured.Name,
                        ProcessingTypeCode = g.Key.ProcessingType?.Code,
                        ProcessingTypeName = g.Key.ProcessingType?.Name ?? "Others",
                        ProcessingFactor = g.Average(ipc => ipc.ProcessingFactor),
                        ProcessingCorrectionFactor = g.AverageOrZero(ipc => ipc.ProcessingCorrectionFactor),
                        MeanAll = g.Sum(ipc => ipc.Exposure * ipc.SamplingWeight) / sumSamplingWeights,
                        Contribution = g.Sum(ipc => ipc.Intake * ipc.SamplingWeight) / totalIntake,
                        NumberOfPositives = g.Distinct(ipc => ipc.SimulatedIndividualId).Count(),
                        MeanPositives = g.Sum(ipc => ipc.Exposure * ipc.SamplingWeight) / g.Sum(ipc => ipc.SamplingWeight),
                        Contributions = [],
                    });
                    return intakesPerFoodPerCompound;
                })
                .OrderByDescending(c => c.Contribution)
                .ToList();
        }

        protected void updateContributions(List<FoodAsMeasuredSubstanceProcessingTypeRecord> bootstrapRecords) {
            var recordsLookup = Records.ToDictionary(r => (r.FoodCode, r.SubstanceCode, r.ProcessingTypeCode));
            foreach (var bootstrapRecord in bootstrapRecords) {
                if (!recordsLookup.TryGetValue((bootstrapRecord.FoodCode, bootstrapRecord.SubstanceCode, bootstrapRecord.ProcessingTypeCode), out var r)) {
                    var contribution = bootstrapRecord.Contribution;
                    r = new FoodAsMeasuredSubstanceProcessingTypeRecord {
                        SubstanceCode = bootstrapRecord.SubstanceCode,
                        SubstanceName = bootstrapRecord.SubstanceName,
                        FoodCode = bootstrapRecord.FoodCode,
                        FoodName = bootstrapRecord.FoodName,
                        ProcessingTypeCode = bootstrapRecord.ProcessingTypeCode,
                        ProcessingTypeName = bootstrapRecord.ProcessingTypeName,
                        ProcessingFactor = bootstrapRecord.ProcessingFactor,
                        ProcessingCorrectionFactor = bootstrapRecord.ProcessingCorrectionFactor,
                        Contribution = 0,
                        Contributions = Enumerable.Repeat(0d, UncertaintyCycles - 1).ToList(),
                    };
                    Records.Add(r);
                }
                r.Contributions.Add(bootstrapRecord.Contribution * 100);
            }
            foreach (var record in Records) {
                if (record.Contributions.Count < UncertaintyCycles) {
                    record.Contributions.Add(0);
                }
            }
        }

        protected void setUncertaintyBounds() {
            foreach (var item in Records) {
                item.UncertaintyLowerBound = UncertaintyLowerBound;
                item.UncertaintyUpperBound = UncertaintyUpperBound;
            }
        }
    }
}

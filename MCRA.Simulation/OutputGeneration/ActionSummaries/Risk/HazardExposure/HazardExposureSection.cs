using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposureSection : SummarySection {

        public override bool SaveTemporaryData => true;
        public List<TargetUnit> TargetUnits { get; set; }
        public List<(ExposureTarget Target, List<HazardExposureRecord> Records)> HazardExposureRecords { get; set; } = new();
        public HealthEffectType HealthEffectType { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public double ConfidenceInterval { get; set; }
        public double Threshold { get; set; }
        public int NumberOfLabels { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public bool HasUncertainty { get; set; }

        /// <summary>
        /// Summarizes hazard versus exposure charts.
        /// </summary>
        public void Summarize(
            ICollection<ExposureTarget> targets,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            HealthEffectType healthEffectType,
            ICollection<Compound> substances,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationsModelsCollections,
            IHazardCharacterisationModel referenceDose,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            double confidenceInterval,
            double threshold,
            int numberOfLabels,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            NumberOfLabels = numberOfLabels;
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            HealthEffectType = healthEffectType;
            RiskMetricType = riskMetricType;
            TargetUnits = new();

            foreach (var target in targets) {
                var hazardExposureRecords = new List<HazardExposureRecord>();
                TargetUnit targetUnit = null;
                if (targets.Count == 1 && individualEffects != null && referenceDose != null) {
                    targetUnit = new TargetUnit(target, referenceDose.DoseUnit);

                    // Overal risk record (cumulative or single-substance)
                    // Only add when there is only one target
                    var cumulativeRecord = getCumulativeHazardExposureRecords(
                        target,
                        individualEffects,
                        referenceDose,
                        riskMetricCalculationType,
                        isCumulative
                    );
                    hazardExposureRecords.Add(cumulativeRecord);
                }

                // Target substance records
                if (individualEffectsBySubstanceCollections?.Any() ?? false) {
                    var hazardCharacterisationsModelsCollection = hazardCharacterisationsModelsCollections
                        .Single(c => c.TargetUnit?.Target == target);
                    var hazardCharacterisations = hazardCharacterisationsModelsCollection.HazardCharacterisationModels;
                    targetUnit = targetUnit ?? hazardCharacterisationsModelsCollection.TargetUnit;

                    var targetRecords = getSubstanceHazardExposureRecords(
                        target,
                        individualEffectsBySubstanceCollections,
                        hazardCharacterisations,
                        substances,
                        riskMetricCalculationType,
                        referenceDose,
                        isCumulative
                    );
                    hazardExposureRecords.AddRange(targetRecords);                    
                }

                // Add target records
                HazardExposureRecords.Add((target, hazardExposureRecords));
                TargetUnits.Add(targetUnit);
            }
        }

        /// <summary>
        /// Summarizes uncertainty results.
        /// </summary>
        public void SummarizeUncertainty(
            ICollection<ExposureTarget> targets,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            List<IndividualEffect> individualEffects,
            ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationsModelsCollection,
            ICollection<Compound> substances,
            IHazardCharacterisationModel referenceDose,
            RiskMetricCalculationType riskMetricCalculationType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isCumulative
        ) {
            HasUncertainty = true;

            foreach (var target in targets) {
                var hazardExposureRecords = new List<HazardExposureRecord>();

                // Overal risk record (cumulative or single-substance)
                if (targets.Count == 1 && individualEffects != null && referenceDose != null) {
                    // Only add when there is only one target
                    var cumulativeRecord = getCumulativeHazardExposureRecords(
                        target,
                        individualEffects,
                        referenceDose,
                        riskMetricCalculationType,
                        isCumulative
                    );
                    hazardExposureRecords.Add(cumulativeRecord);
                }

                // Target substance records
                if (individualEffectsBySubstanceCollections?.Any() ?? false) {
                    var hazardCharacterisationModel = hazardCharacterisationsModelsCollection
                    .SingleOrDefault(c => c.TargetUnit?.Target == target)?
                    .HazardCharacterisationModels;
                    var targetRecords = getSubstanceHazardExposureRecords(
                        target,
                        individualEffectsBySubstanceCollections,
                        hazardCharacterisationModel,
                        substances,
                        riskMetricCalculationType,
                        referenceDose,
                        isCumulative
                    );
                    hazardExposureRecords.AddRange(targetRecords);
                }

                var recordsLookup = HazardExposureRecords
                    .SingleOrDefault(c => c.Target == target).Records
                    .ToDictionary(r => r.SubstanceCode);
                foreach (var item in hazardExposureRecords) {
                    var record = recordsLookup[item.SubstanceCode];
                    record.UncertaintyLowerLimit = uncertaintyLowerBound;
                    record.UncertaintyUpperLimit = uncertaintyUpperBound;
                    recordsLookup[item.SubstanceCode].ExposurePercentilesAll
                        .AddUncertaintyValues(new List<double> { item.LowerAllExposure, item.MedianAllExposure, item.UpperAllExposure });
                    recordsLookup[item.SubstanceCode].ExposurePercentilesPositives
                        .AddUncertaintyValues(new List<double> { item.LowerExposure, item.MedianExposure, item.UpperExposure });
                    recordsLookup[item.SubstanceCode].HazardCharacterisationPercentilesAll
                        .AddUncertaintyValues(new List<double> { item.LowerHc, item.MedianHc, item.UpperHc });
                    recordsLookup[item.SubstanceCode].RiskPercentilesPositives
                        .AddUncertaintyValues(new List<double> { item.LowerRisk, item.MedianRisk, item.UpperRisk });
                    recordsLookup[item.SubstanceCode].RiskPercentilesUncertainties
                        .AddUncertaintyValues(new List<double> { item.LowerRisk, item.MedianRisk, item.UpperRisk });
                }
            }
        }

        private List<HazardExposureRecord> getSubstanceHazardExposureRecords(
            ExposureTarget target,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> IndividualEffects)> individualEffectsBySubstanceCollections,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            ICollection<Compound> substances,
            RiskMetricCalculationType riskMetricCalculationType,
            IHazardCharacterisationModel referenceDose,
            bool isCumulative
        ) {
            var records = new List<HazardExposureRecord>();
            var targetIndividualEffects = individualEffectsBySubstanceCollections?
                .SingleOrDefault(c => c.Target == target)
                .IndividualEffects;
            if (targetIndividualEffects?.Any() ?? false) {
                foreach (var substance in substances) {
                    if (targetIndividualEffects.TryGetValue(substance, out var results)) {
                        var record = calculateHazardExposure(
                            target,
                            results,
                            (isCumulative && riskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) ? referenceDose : hazardCharacterisations[substance],
                            substance,
                            false
                        );
                        records.Add(record);
                    }
                }
            }
            return records
                .OrderByDescending(c => c.UpperExposure)
                .ToList();
        }

        private HazardExposureRecord getCumulativeHazardExposureRecords(
            ExposureTarget target,
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel hazardCharacterisation,
            RiskMetricCalculationType riskMetricCalculationType,
            bool isCumulative
        ) {
            Compound riskReference;
            if (isCumulative) {
                if (riskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.RpfWeighted.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                } else {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.SumOfRiskRatios.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                }
            } else {
                riskReference = hazardCharacterisation.Substance;
            }
            var result = calculateHazardExposure(
                target,
                individualEffects,
                hazardCharacterisation,
                riskReference,
                isCumulative
            );
            return result;
        }

        /// <summary>
        /// Calculate statistics for CED vs Exposure plot (Risk 21).
        /// </summary>
        private HazardExposureRecord calculateHazardExposure(
            ExposureTarget target,
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel hazardCharacterisation,
            Compound substance,
            bool isCumulativeRecord
        ) {
            var pLower = (100 - ConfidenceInterval) / 2;
            var percentages = new double[] { pLower, 50, 100 - pLower };
            var allWeights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentilesAllExposure = individualEffects
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(allWeights, percentages);

            var percentilesAllCed = individualEffects
                .Select(c => c.CriticalEffectDose)
                .PercentilesWithSamplingWeights(allWeights, percentages);

            var weights = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.SamplingWeight).ToList();

            var percentilesExposure = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(weights, percentages);

            var logHazards = individualEffects.Where(c => c.IsPositive)
                .Select(c => Math.Log(c.CriticalEffectDose))
                .ToList();

            var logExposures = individualEffects.Where(c => c.IsPositive)
                .Select(c => Math.Log(c.Exposure))
                .ToList();

            var averageLogExposure = individualEffects
                .Where(c => c.IsPositive)
                .Sum(c => Math.Log(c.Exposure) * c.SamplingWeight) / weights.Sum();

            var percentilesRiskAll = individualEffects
                .Select(c => c.ExposureHazardRatio)
                .PercentilesWithSamplingWeights(allWeights, percentages);

            var percentilesRiskPositives = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.ExposureHazardRatio)
                .PercentilesWithSamplingWeights(weights, percentages);

            var percentilesRiskUncertainties = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.ExposureHazardRatio)
                .PercentilesWithSamplingWeights(weights, percentages);

            var record = new HazardExposureRecord() {
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                BiologicalMatrix = target != null && target.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? target.BiologicalMatrix.GetDisplayName() : null,
                ExpressionType = target != null && target.ExpressionType != ExpressionType.None
                    ? target.ExpressionType.GetDisplayName() : null,
                IsCumulativeRecord = isCumulativeRecord,
                NominalHazardCharacterisation = hazardCharacterisation.Value,
                MeanHc = logHazards.Any() ? Math.Exp(logHazards.Average()) : double.NaN,
                StDevHc = logHazards.Any() ? Math.Sqrt(logHazards.Variance()) : double.NaN,
                StDevExposure = logExposures.Any() ? Math.Sqrt(logExposures.Variance(weights)) : double.NaN,
                PercentagePositives = 100d * weights.Sum() / allWeights.Sum(),
                MeanExposure = individualEffects.Any() ? individualEffects.Average(c => c.Exposure) : double.NaN,
                ExposurePercentilesAll = new UncertainDataPointCollection<double>() {
                    XValues = percentages,
                    ReferenceValues = percentilesAllExposure,
                },
                ExposurePercentilesPositives = new UncertainDataPointCollection<double>() {
                    XValues = percentages,
                    ReferenceValues = percentilesExposure,
                },
                HazardCharacterisationPercentilesAll = new UncertainDataPointCollection<double>() {
                    XValues = percentages,
                    ReferenceValues = percentilesAllCed,
                },
                RiskPercentilesPositives = new UncertainDataPointCollection<double>() {
                    XValues = percentages,
                    ReferenceValues = percentilesRiskPositives,
                },
                RiskPercentilesUncertainties = new UncertainDataPointCollection<double>() {
                    XValues = percentages,
                    ReferenceValues = percentilesRiskUncertainties,
                },
                LowerAllRisk = percentilesRiskAll[0],
                UpperAllRisk = percentilesRiskAll[2],
                MedianAllRisk = percentilesRiskAll[1],
                TargetUnit = hazardCharacterisation.DoseUnit.GetShortDisplayName(), 
            };
            return record;
        }
    }
}

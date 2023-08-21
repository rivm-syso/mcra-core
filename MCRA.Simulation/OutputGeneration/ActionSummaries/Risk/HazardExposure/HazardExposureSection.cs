using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using System.Collections.Concurrent;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardExposureSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<HazardExposureRecord> HazardExposureRecords { get; set; }
        public HealthEffectType HealthEffectType { get; set; }
        public RiskMetricType RiskMetricType { get; set; }
        public double ConfidenceInterval { get; set; }
        public double Threshold { get; set; }
        public int NumberOfLabels { get; set; }
        public int NumberOfSubstances { get; set; }
        public double UncertaintyLowerLimit { get; set; }
        public double UncertaintyUpperLimit { get; set; }
        public bool HasUncertainty { get; set; } = false;

        /// <summary>
        /// Summarizes Hazard versus Exposure charts.
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="individualEffects"></param>
        /// <param name="healthEffectType"></param>
        /// <param name="substances"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="riskMetricType"></param>
        /// <param name="confidenceInterval"></param>
        /// <param name="threshold"></param>
        /// <param name="numberOfLabels"></param>
        /// <param name="numberOfSubstances"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void Summarize(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            HealthEffectType healthEffectType,
            ICollection<Compound> substances,
            Compound referenceSubstance,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            double confidenceInterval,
            double threshold,
            int numberOfLabels,
            int numberOfSubstances,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool onlyCumulativeOutput = false
        ) {
            NumberOfLabels = numberOfLabels;
            NumberOfSubstances = numberOfSubstances;
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;
            ConfidenceInterval = confidenceInterval;
            Threshold = threshold;
            HealthEffectType = healthEffectType;
            RiskMetricType = riskMetricType;
            HazardExposureRecords = getHazardExposureRecords(
                individualEffectsBySubstance,
                individualEffects,
                hazardCharacterisations,
                substances,
                referenceSubstance,
                riskMetricType,
                riskMetricCalculationType,
                onlyCumulativeOutput
            );
        }

        /// <summary>
        /// Summarizes uncertainty for CED vs Exposure charts.
        /// </summary>
        /// <param name="individualEffectsBySubstance"></param>
        /// <param name="individualEffects"></param>
        /// <param name="hazardCharacterisations"></param>
        /// <param name="substances"></param>
        /// <param name="reference"></param>
        /// <param name="riskMetricType"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
            Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
            List<IndividualEffect> individualEffects,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
            ICollection<Compound> substances,
            Compound reference,
            RiskMetricType riskMetricType,
            RiskMetricCalculationType riskMetricCalculationType,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool onlyCumulativeOutput = false
        ) {
            HasUncertainty = true;
            var recordLookup = HazardExposureRecords.ToDictionary(r => r.SubstanceCode);
            var records = getHazardExposureRecords(
                individualEffectsBySubstance,
                individualEffects,
                hazardCharacterisations,
                substances,
                reference,
                riskMetricType,
                riskMetricCalculationType,
                onlyCumulativeOutput
            );
            foreach (var item in records) {
                var record = recordLookup[item.SubstanceCode];
                record.UncertaintyLowerLimit = uncertaintyLowerBound;
                record.UncertaintyUpperLimit = uncertaintyUpperBound;
                recordLookup[item.SubstanceCode].ExposurePercentilesAll
                    .AddUncertaintyValues(new List<double> { item.LowerAllExposure, item.MedianAllExposure, item.UpperAllExposure });
                recordLookup[item.SubstanceCode].ExposurePercentilesPositives
                    .AddUncertaintyValues(new List<double> { item.LowerExposure, item.MedianExposure, item.UpperExposure });
                recordLookup[item.SubstanceCode].HazardCharacterisationPercentilesAll
                    .AddUncertaintyValues(new List<double> { item.LowerHc, item.MedianHc, item.UpperHc });
                recordLookup[item.SubstanceCode].RiskPercentilesPositives
                    .AddUncertaintyValues(new List<double> { item.LowerRisk, item.MedianRisk, item.UpperRisk });
                recordLookup[item.SubstanceCode].RiskPercentilesUncertainties
                    .AddUncertaintyValues(new List<double> { item.LowerRisk, item.MedianRisk, item.UpperRisk });
            }
        }

        private List<HazardExposureRecord> getHazardExposureRecords(
             Dictionary<Compound, List<IndividualEffect>> individualEffectsBySubstance,
             List<IndividualEffect> individualEffects,
             IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations,
             ICollection<Compound> substances,
             Compound reference,
             RiskMetricType riskMetricType,
             RiskMetricCalculationType riskMetricCalculationType,
             bool onlyCumulativeOutput
        ) {
            var records = new ConcurrentBag<HazardExposureRecord>();
            if (substances.Count > 1 && individualEffects != null || onlyCumulativeOutput) {
                Compound riskReference = null;
                if (riskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.RpfWeighted.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                    records.Add(
                        calculateHazardExposure(
                            individualEffects,
                            hazardCharacterisations[reference],
                            riskReference,
                            true,
                            riskMetricType
                        )
                    );
                } else if (riskMetricCalculationType == RiskMetricCalculationType.SumRatios) {
                    riskReference = new Compound() {
                        Name = RiskReferenceCompoundType.SumOfRiskRatios.GetDisplayName(),
                        Code = "CUMULATIVE",
                    };
                }
            }
            if (!onlyCumulativeOutput) {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1000 }; //, CancellationToken = cancelToken };
                Parallel.ForEach(substances, parallelOptions, substance => {
                    var record = calculateHazardExposure(
                            individualEffectsBySubstance[substance],
                            hazardCharacterisations[substance],
                            substance,
                            false,
                            riskMetricType
                        );
                    records.Add(record);
                });
            }
            return records.OrderByDescending(c => c.UpperExposure).ToList();
        }

        /// <summary>
        /// Calculate statistics for CED vs Exposure plot (Risk 21).
        /// </summary>
        /// <param name="individualEffects"></param>
        /// <param name="compound"></param>
        /// <param name="hazardCharacterisation"></param>
        /// <param name="riskMetricType"></param>
        /// <returns></returns>
        private HazardExposureRecord calculateHazardExposure(
            List<IndividualEffect> individualEffects,
            IHazardCharacterisationModel hazardCharacterisation,
            Compound compound,
            bool isCumulativeRecord,
            RiskMetricType riskMetricType
        ) {
            var pLower = (100 - ConfidenceInterval) / 2;
            var percentages = new double[] { pLower, 50, 100 - pLower };
            var allWeights = individualEffects.Select(c => c.SamplingWeight).ToList();
            var percentilesAllExposure = individualEffects
                .Select(c => c.ExposureConcentration)
                .PercentilesWithSamplingWeights(allWeights, percentages);

            var percentilesAllCed = individualEffects
                .Select(c => c.CriticalEffectDose)
                .PercentilesWithSamplingWeights(allWeights, percentages);

            var weights = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.SamplingWeight).ToList();

            var percentilesExposure = individualEffects
                .Where(c => c.IsPositive)
                .Select(c => c.ExposureConcentration)
                .PercentilesWithSamplingWeights(weights, percentages);

            var logHazards = individualEffects.Where(c => c.IsPositive)
                .Select(c => Math.Log(c.CriticalEffectDose))
                .ToList();

            var logExposures = individualEffects.Where(c => c.IsPositive)
                .Select(c => Math.Log(c.ExposureConcentration))
                .ToList();

            var averageLogExposure = individualEffects
                .Where(c => c.IsPositive)
                .Sum(c => Math.Log(c.ExposureConcentration) * c.SamplingWeight) / weights.Sum();

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
                HealthEffectType = HealthEffectType,
                SubstanceName = compound.Name,
                SubstanceCode = compound.Code,
                IsCumulativeRecord = isCumulativeRecord,
                NominalHazardCharacterisation = hazardCharacterisation.Value,
                MeanHc = logHazards.Any() ? Math.Exp(logHazards.Average()) : double.NaN,
                StDevHc = logHazards.Any() ? Math.Sqrt(logHazards.Variance()) : double.NaN,
                StDevExposure = logExposures.Any() ? Math.Sqrt(logExposures.Variance(weights)) : double.NaN,
                PercentagePositives = 100d * weights.Sum() / allWeights.Sum(),
                MeanExposure = individualEffects.Any() ? individualEffects.Average(c => c.ExposureConcentration) : double.NaN,
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
            };
            return record;
        }
    }
}

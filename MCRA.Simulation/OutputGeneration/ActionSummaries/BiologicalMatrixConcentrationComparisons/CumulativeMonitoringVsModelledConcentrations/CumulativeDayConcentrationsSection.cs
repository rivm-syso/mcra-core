using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class CumulativeDayConcentrationsSection : SummarySection {

        public List<BiologicalMatrixConcentrationPercentilesRecord> Records { get; set; } = new();
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }

        public void Summarize(
            ICollection<ITargetIndividualDayExposure> cumulativeIndividualDayTargetExposures,
            ICollection<HbmCumulativeIndividualDayCollection> cumulativeHbmIndividualDayCollections,
            Compound referenceSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetExposureUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var boxPlotPercentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var result = new List<BiologicalMatrixConcentrationPercentilesRecord>();
            foreach (var collection in cumulativeHbmIndividualDayCollections) {
                {
                    var concentrationAlignmentFactor = collection.TargetUnit
                        .GetAlignmentFactor(targetExposureUnit, referenceSubstance.MolecularMass, double.NaN);

                    var hbmConcentrations = collection.HbmCumulativeIndividualDayConcentrations
                        .Select(c => (
                            samplingWeight: c.Individual.SamplingWeight,
                            concentration: c.CumulativeConcentration * concentrationAlignmentFactor
                        ))
                        .ToList();

                    // All individual days
                    var weightsAll = hbmConcentrations
                        .Select(c => c.samplingWeight)
                        .ToList();
                    var percentilesAll = hbmConcentrations
                        .Select(c => c.concentration)
                        .PercentilesWithSamplingWeights(weightsAll, percentages);
                    var boxPlotPercentiles = hbmConcentrations
                        .Select(c => c.concentration)
                        .PercentilesWithSamplingWeights(weightsAll, boxPlotPercentages)
                        .ToList();

                    // Positive individual days
                    var positives = hbmConcentrations
                        .Where(c => c.concentration > 0)
                        .ToList();
                    var weightsPositives = positives
                        .Select(c => c.samplingWeight)
                        .ToList();
                    var percentilesPositives = positives
                        .Select(c => c.concentration)
                        .PercentilesWithSamplingWeights(weightsPositives, percentages);

                    var record = new BiologicalMatrixConcentrationPercentilesRecord {
                        SubstanceCode = "Cumulative",
                        SubstanceName = "Cumulative",
                        Type = "Monitoring",
                        BiologicalMatrix = collection.TargetUnit.BiologicalMatrix.GetDisplayName(),
                        NumberOfPositives = positives.Count,
                        PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
                        MeanPositives = hbmConcentrations.Sum(c => c.concentration * c.samplingWeight) / weightsPositives.Sum(),
                        LowerPercentilePositives = percentilesPositives[0],
                        MedianPositives = percentilesPositives[1],
                        UpperPercentilePositives = percentilesPositives[2],
                        LowerPercentileAll = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        UpperPercentileAll = percentilesAll[2],
                        BoxPlotPercentiles = boxPlotPercentiles.ToList(),
                        MinPositives = positives.Any() ? positives.Min(r => r.concentration) : double.NaN,
                        MaxPositives = positives.Any() ? positives.Max(r => r.concentration) : double.NaN,
                    };
                    result.Add(record);
                }

                {
                    var targetConcentrations = cumulativeIndividualDayTargetExposures
                        .Select(r => (
                            samplingWeight: r.IndividualSamplingWeight,
                            concentration: r.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, false)
                        ))
                        .ToList();

                    // All individual days
                    var weightsAll = targetConcentrations
                        .Select(c => c.samplingWeight)
                        .ToList();
                    var percentilesAll = targetConcentrations
                        .Select(c => c.concentration)
                        .PercentilesWithSamplingWeights(weightsAll, percentages);
                    var boxPlotPercentiles = targetConcentrations
                        .Select(c => c.concentration)
                        .PercentilesWithSamplingWeights(null, boxPlotPercentages)
                        .ToList();

                    // Positive individual days
                    var positives = targetConcentrations
                        .Where(c => c.concentration > 0)
                        .ToList();
                    var weightsPositives = positives
                        .Select(c => c.samplingWeight)
                        .ToList();
                    var percentilesPositives = positives
                        .Select(c => c.concentration)
                        .PercentilesWithSamplingWeights(weightsPositives, percentages);

                    var record = new BiologicalMatrixConcentrationPercentilesRecord {
                        SubstanceCode = "Cumulative",
                        SubstanceName = "Cumulative",
                        Type = "Modelled",
                        BiologicalMatrix = targetExposureUnit.BiologicalMatrix.GetDisplayName(),
                        NumberOfPositives = positives.Count,
                        PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
                        MeanPositives = targetConcentrations.Sum(c => c.concentration * c.samplingWeight) / weightsPositives.Sum(),
                        LowerPercentilePositives = percentilesPositives[0],
                        MedianPositives = percentilesPositives[1],
                        UpperPercentilePositives = percentilesPositives[2],
                        LowerPercentileAll = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        UpperPercentileAll = percentilesAll[2],
                        BoxPlotPercentiles = boxPlotPercentiles,
                        MinPositives = positives.Any() ? positives.Min(r => r.concentration) : double.NaN,
                        MaxPositives = positives.Any() ? positives.Max(r => r.concentration) : double.NaN,
                    };
                    result.Add(record);
                }

                result = result
                    .Where(r => r.MeanPositives > 0)
                    .OrderBy(s => s.ExposureRoute)
                    .ToList();

                Records.AddRange(result);
            }
        }
    }
}

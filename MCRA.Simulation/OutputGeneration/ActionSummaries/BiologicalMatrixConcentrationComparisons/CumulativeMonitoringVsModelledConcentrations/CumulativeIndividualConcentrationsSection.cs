using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class CumulativeIndividualConcentrationsSection : SummarySection {

        public List<BiologicalMatrixConcentrationPercentilesRecord> Records { get; set; } = new();
        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }
        public string ExposureTarget { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> targetExposures,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
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
            {
                ExposureTarget = hbmCumulativeIndividualCollection.TargetUnit.ExposureUnit.GetShortDisplayName();
                var concentrationAlignmentFactor = hbmCumulativeIndividualCollection.TargetUnit
                    .GetAlignmentFactor(targetExposureUnit, referenceSubstance.MolecularMass, double.NaN);

                var hbmConcentrations = hbmCumulativeIndividualCollection.HbmCumulativeIndividualConcentrations
                    .Select(c => (
                        SamplingWeight: c.Individual.SamplingWeight,
                        Concentration: c.CumulativeConcentration * concentrationAlignmentFactor
                    ))
                    .ToList();

                // All individuals
                var weightsAll = hbmConcentrations
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentilesAll = hbmConcentrations
                    .Select(c => c.Concentration)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var boxPlotPercentiles = hbmConcentrations
                    .Select(c => c.Concentration)
                    .PercentilesWithSamplingWeights(weightsAll, boxPlotPercentages)
                    .ToList();

                // Positive individuals
                var positives = hbmConcentrations
                    .Where(r => r.Concentration > 0)
                    .ToList();
                var weightsPositives = positives
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentilesPositives = positives
                    .Select(c => c.Concentration)
                    .PercentilesWithSamplingWeights(weightsPositives, percentages);

                var record = new BiologicalMatrixConcentrationPercentilesRecord {
                    SubstanceCode = "Cumulative",
                    SubstanceName = "Cumulative",
                    Type = "Monitoring",
                    BiologicalMatrix = hbmCumulativeIndividualCollection.TargetUnit.BiologicalMatrix.GetDisplayName(),
                    Unit = hbmCumulativeIndividualCollection.TargetUnit.ExposureUnit.GetShortDisplayName(),
                    NumberOfPositives = positives.Count,
                    PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
                    MeanPositives = hbmConcentrations.Sum(c => c.Concentration * c.SamplingWeight) / weightsPositives.Sum(),
                    LowerPercentilePositives = percentilesPositives[0],
                    MedianPositives = percentilesPositives[1],
                    UpperPercentilePositives = percentilesPositives[2],
                    LowerPercentileAll = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    UpperPercentileAll = percentilesAll[2],
                    BoxPlotPercentiles = boxPlotPercentiles.ToList(),
                    MinPositives = positives.Any() ? positives.Min(r => r.Concentration) : double.NaN,
                    MaxPositives = positives.Any() ? positives.Max(r => r.Concentration) : double.NaN,
                };
                result.Add(record);
            }


            {
                var targetConcentrations = targetExposures
                    .Select(r => (
                        SamplingWeight: r.IndividualSamplingWeight,
                        Concentration: r.GetTotalExposureAtTarget(targetExposureUnit.Target, relativePotencyFactors, membershipProbabilities)
                        ))
                    .ToList();

                // All individuals
                var weightsAll = targetConcentrations
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentilesAll = targetConcentrations
                   .Select(c => c.Concentration)
                   .PercentilesWithSamplingWeights(weightsAll, percentages);
                var boxPlotPercentiles = targetConcentrations
                    .Select(c => c.Concentration)
                    .PercentilesWithSamplingWeights(null, boxPlotPercentages)
                    .ToList();

                // Positive individuals
                var positives = targetConcentrations
                    .Where(c => c.Concentration > 0)
                    .ToList();
                var weightsPositives = positives
                    .Select(c => c.SamplingWeight)
                    .ToList();
                var percentilesPositives = positives
                    .Select(c => c.Concentration)
                    .PercentilesWithSamplingWeights(weightsPositives, percentages);

                var record = new BiologicalMatrixConcentrationPercentilesRecord {
                    SubstanceCode = "Cumulative",
                    SubstanceName = "Cumulative",
                    Type = "Modelled",
                    BiologicalMatrix = targetExposureUnit.BiologicalMatrix.GetDisplayName(),
                    Unit = targetExposureUnit.GetShortDisplayName(),
                    NumberOfPositives = positives.Count,
                    PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
                    MeanPositives = targetConcentrations.Sum(c => c.Concentration * c.SamplingWeight) / weightsPositives.Sum(),
                    LowerPercentilePositives = percentilesPositives[0],
                    MedianPositives = percentilesPositives[1],
                    UpperPercentilePositives = percentilesPositives[2],
                    LowerPercentileAll = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    UpperPercentileAll = percentilesAll[2],
                    BoxPlotPercentiles = boxPlotPercentiles,
                    MinPositives = positives.Any() ? positives.Min(r => r.Concentration) : double.NaN,
                    MaxPositives = positives.Any() ? positives.Max(r => r.Concentration) : double.NaN,
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


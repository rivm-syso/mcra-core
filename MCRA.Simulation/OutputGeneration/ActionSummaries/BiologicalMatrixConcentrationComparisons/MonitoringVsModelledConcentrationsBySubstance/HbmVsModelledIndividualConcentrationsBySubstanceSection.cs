using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmVsModelledIndividualConcentrationsBySubstanceSection : SummarySection {

        public List<BiologicalMatrixConcentrationPercentilesRecord> HbmBoxPlotRecords { get; set; } = [];

        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }
        public string ExposureTarget { get; set; }
        public void Summarize(
            ICollection<AggregateIndividualExposure> targetExposures,
            ICollection<HbmIndividualCollection> hbmIndividualConcentrationsCollections,
            ICollection<Compound> substances,
            TargetUnit targetExposureUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;

            var result = new List<BiologicalMatrixConcentrationPercentilesRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var boxPlotPercentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            foreach (var collection in hbmIndividualConcentrationsCollections) {
                ExposureTarget = collection.TargetUnit.ExposureUnit.GetShortDisplayName();
                foreach (var substance in substances) {
                    {
                        // TODO. 10-03-2013, see issue https://git.wur.nl/Biometris/mcra-dev/MCRA-Issues/-/issues/1524
                        var concentrationAlignmentFactor = collection.TargetUnit
                            .GetAlignmentFactor(targetExposureUnit, substance.MolecularMass, double.NaN);

                        var hbmConcentrations = collection.HbmIndividualConcentrations
                            .Select(c => (
                                SamplingWeight: c.Individual.SamplingWeight,
                                concentration: c.ConcentrationsBySubstance[substance].Exposure * concentrationAlignmentFactor
                            ))
                            .ToList();

                        // All individuals
                        var weightsAll = hbmConcentrations
                            .Select(c => c.SamplingWeight)
                            .ToList();
                        var percentilesAll = hbmConcentrations
                            .Select(c => c.concentration)
                            .PercentilesWithSamplingWeights(weightsAll, percentages)
                            .ToList();
                        var boxPlotPercentiles = hbmConcentrations
                            .Select(c => c.concentration)
                            .PercentilesWithSamplingWeights(weightsAll, boxPlotPercentages)
                            .ToList();

                        // Positive individuals
                        var positives = hbmConcentrations
                            .Where(r => r.concentration > 0)
                            .ToList();
                        var weightsPositives = positives
                            .Select(c => c.SamplingWeight).ToList();
                        var percentilesPositives = positives
                            .Select(c => c.concentration)
                            .PercentilesWithSamplingWeights(weightsPositives, percentages);

                        var record = new BiologicalMatrixConcentrationPercentilesRecord() {
                            SubstanceCode = substance.Code,
                            SubstanceName = substance.Name,
                            Type = "Monitoring",
                            BiologicalMatrix = collection.TargetUnit.BiologicalMatrix.GetDisplayName(),
                            Unit = collection.TargetUnit.ExposureUnit.GetShortDisplayName(),
                            NumberOfPositives = positives.Count,
                            PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
                            MeanPositives = hbmConcentrations.Sum(c => c.concentration * c.SamplingWeight) / weightsPositives.Sum(),
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

                    {
                        var targetConcentrations = targetExposures
                            .Select(r => (
                                samplingWeight: r.IndividualSamplingWeight,
                                targetExposure: r.GetSubstanceTargetExposure(targetExposureUnit.Target, substance)
                            ))
                            .Select(r => (
                                SamplingWeight: r.samplingWeight,
                                concentration: r.targetExposure.Exposure
                                )
                            )
                            .ToList();

                        // All individuals
                        var weightsAll = targetConcentrations
                            .Select(c => c.SamplingWeight)
                            .ToList();
                        var percentilesAll = targetConcentrations
                           .Select(c => c.concentration)
                           .PercentilesWithSamplingWeights(weightsAll, percentages);
                        var boxPlotPercentiles = targetConcentrations
                            .Select(c => c.concentration)
                            .PercentilesWithSamplingWeights(null, boxPlotPercentages)
                            .ToList();

                        // Positive individuals
                        var positives = targetConcentrations
                            .Where(c => c.concentration > 0)
                            .ToList();
                        var weightsPositives = positives
                            .Select(c => c.SamplingWeight)
                            .ToList();
                        var percentilesPositives = positives
                           .Select(c => c.concentration)
                           .PercentilesWithSamplingWeights(weightsPositives, percentages);

                        var record = new BiologicalMatrixConcentrationPercentilesRecord() {
                            SubstanceCode = substance.Code,
                            SubstanceName = substance.Name,
                            Type = "Modelled",
                            BiologicalMatrix = targetExposureUnit.BiologicalMatrix.GetDisplayName(),
                            Unit = targetExposureUnit.GetShortDisplayName(),
                            NumberOfPositives = positives.Count,
                            PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
                            MeanPositives = targetConcentrations.Sum(c => c.concentration * c.SamplingWeight) / weightsPositives.Sum(),
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
                }
                HbmBoxPlotRecords.AddRange(result);
            }
        }
    }
}

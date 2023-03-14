using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmVsModelledIndividualConcentrationsBySubstanceSection : SummarySection {

        public List<BiologicalMatrixConcentrationPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public double LowerPercentage { get; set; }
        public double UpperPercentage { get; set; }

        public void Summarize(
            ICollection<ITargetIndividualExposure> targetExposures,
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations,
            ICollection<Compound> substances,
            TargetUnit targetExposureUnit,
            List<TargetUnit> hbmConcentrationUnits,
            double lowerPercentage,
            double upperPercentage,
            string biologicalMatrix
        ) {
            LowerPercentage = lowerPercentage;
            UpperPercentage = upperPercentage;

            var result = new List<BiologicalMatrixConcentrationPercentilesRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            var boxPlotPercentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };

            foreach (var substance in substances) {
                {
                    // TODO. 10-03-2013, see issue https://git.wur.nl/Biometris/mcra-dev/MCRA-Issues/-/issues/1524
                    System.Diagnostics.Debug.Assert(hbmConcentrationUnits.Count > 0);
                    var firstHhbmConcentrationUnit = hbmConcentrationUnits[0];

                    var concentrationAlignmentFactor = firstHhbmConcentrationUnit
                        .GetAlignmentFactor(targetExposureUnit, substance.MolecularMass, double.NaN);

                    var hbmConcentrations = hbmIndividualConcentrations
                        .Select(c => (
                            samplingWeight: c.Individual.SamplingWeight,
                            concentration: c.ConcentrationsBySubstance[substance].Concentration * concentrationAlignmentFactor
                        ))
                        .ToList();

                    // All individuals
                    var weightsAll = hbmConcentrations
                        .Select(c => c.samplingWeight)
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
                        .Select(c => c.samplingWeight).ToList();
                    var percentilesPositives = positives
                        .Select(c => c.concentration)
                        .PercentilesWithSamplingWeights(weightsPositives, percentages);

                    var record = new BiologicalMatrixConcentrationPercentilesRecord() {
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        Type = "Monitoring",
                        BiologicalMatrix = firstHhbmConcentrationUnit.Compartment,
                        NumberOfPositives = positives.Count,
                        PercentagePositives = weightsPositives.Sum() / weightsAll.Sum() * 100D,
                        MeanPositives = hbmConcentrations.Sum(c => c.concentration * c.samplingWeight) / weightsPositives.Sum(),
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
                            compartmentWeight: r.CompartmentWeight,
                            targetExposure: r.GetSubstanceTargetExposure(substance) as ISubstanceTargetExposure
                        ))
                        .Select(r => (
                            samplingWeight: r.samplingWeight,
                            concentration: r.targetExposure.SubstanceAmount / r.compartmentWeight)
                        )
                        .ToList();

                    // All individuals
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

                    // Positive individuals
                    var positives = targetConcentrations
                        .Where(c => c.concentration > 0)
                        .ToList();
                    var weightsPositives = positives
                        .Select(c => c.samplingWeight)
                        .ToList();
                    var percentilesPositives = positives
                       .Select(c => c.concentration)
                       .PercentilesWithSamplingWeights(weightsPositives, percentages);

                    var record = new BiologicalMatrixConcentrationPercentilesRecord() {
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        Type = "Modelled",
                        BiologicalMatrix = targetExposureUnit.Compartment,
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
            }
            HbmBoxPlotRecords = result;
        }
    }
}

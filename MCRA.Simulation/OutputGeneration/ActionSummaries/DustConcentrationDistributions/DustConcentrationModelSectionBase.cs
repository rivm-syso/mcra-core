using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Utils;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public class DustConcentrationModelSectionBase : SummarySection {

        public List<DustConcentrationModelRecord> Records { get; set; }

        protected static DustConcentrationModelRecord createModelSummaryRecord(
            Compound substance,
            ConcentrationModel concentrationModel,
            bool createHistogramBins = true
        ) {
            var record = new DustConcentrationModelRecord {
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Model = concentrationModel.ModelType,
                DesiredModel = concentrationModel.DesiredModelType,
                Unit = concentrationModel.ConcentrationUnit,
                FractionTrueZeros = 1D - concentrationModel.CorrectedWeightedAgriculturalUseFraction,
                FractionCensored = concentrationModel.FractionCensored,
                FractionNonQuantifications = concentrationModel.FractionNonQuantifications,
                FractionNonDetects = concentrationModel.FractionNonDetects,
                FractionPositives = concentrationModel.FractionPositives,
                AgriculturalUseFraction = concentrationModel.WeightedAgriculturalUseFraction,
                CorrectedAgriculturalUseFraction = concentrationModel.CorrectedWeightedAgriculturalUseFraction,
                FractionOfMrl = concentrationModel.ModelType == ConcentrationModelType.MaximumResidueLimit 
                    ? concentrationModel.FractionOfMrl 
                    : null
            };
            double? mu = null;
            double? sigma = null;
            double? maximumResidueLimit = null;
            switch (concentrationModel.ModelType) {
                case ConcentrationModelType.Empirical:
                    mu = null;
                    sigma = null;
                    break;
                case ConcentrationModelType.MaximumResidueLimit:
                    mu = null;
                    sigma = null;
                    maximumResidueLimit = ((CMMaximumResidueLimit)concentrationModel).MaximumResidueLimit;
                    break;
                case ConcentrationModelType.CensoredLogNormal:
                    mu = ((CMCensoredLogNormal)concentrationModel).Mu;
                    sigma = ((CMCensoredLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.ZeroSpikeCensoredLogNormal:
                    mu = ((CMZeroSpikeCensoredLogNormal)concentrationModel).Mu;
                    sigma = ((CMZeroSpikeCensoredLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.NonDetectSpikeLogNormal:
                    mu = ((CMNonDetectSpikeLogNormal)concentrationModel).Mu;
                    sigma = ((CMNonDetectSpikeLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.NonDetectSpikeTruncatedLogNormal:
                    mu = ((CMNonDetectSpikeTruncatedLogNormal)concentrationModel).Mu;
                    sigma = ((CMNonDetectSpikeTruncatedLogNormal)concentrationModel).Sigma;
                    break;
                case ConcentrationModelType.SummaryStatistics:
                    mu = ((CMSummaryStatistics)concentrationModel).Mu;
                    sigma = ((CMSummaryStatistics)concentrationModel).Sigma;
                    break;
                default:
                    break;
            }
            record.Mu = mu;
            record.Sigma = sigma;
            record.MaximumResidueLimit = maximumResidueLimit;
            record.CensoredValuesCount = concentrationModel.Residues?.CensoredValues.Count ?? 0;
            record.TotalMeasurementsCount = concentrationModel.Residues?.NumberOfResidues ?? 0;
            record.LORs = concentrationModel.Residues?.CensoredValues ?? [];
            record.MeanConcentration = concentrationModel.GetDistributionMean();

            if (createHistogramBins && (concentrationModel.Residues?.Positives.Any() ?? false)) {
                // Generate the bins of the positive measurements
                var positiveResidueMeasurements = concentrationModel.Residues.Positives;

                var logPositiveResidueMeasurements = positiveResidueMeasurements
                    .Where(prm => prm > 0)
                    .Select(prm => Math.Log(prm))
                    .ToList();

                var min = BMath.Floor(logPositiveResidueMeasurements.Min(), 1);
                var max = BMath.Ceiling(logPositiveResidueMeasurements.Max(), 1);

                // Correct the minimum bound of the bins to coincide with a LOR value (that is smaller
                // than the minimum of the measured values)
                var lORsSmallerThanMin = record.LORs.Where(lor => lor > 0 
                    && Math.Log(lor) <= logPositiveResidueMeasurements.Min());
                min = (lORsSmallerThanMin.Any()) ? Math.Log(lORsSmallerThanMin.Max()) : min;

                if (min == max) {
                    min = Math.Round(logPositiveResidueMeasurements.Min() - 1);
                    max = Math.Round(logPositiveResidueMeasurements.Max());
                    record.LogPositiveResiduesBins = logPositiveResidueMeasurements.MakeHistogramBins(1, min, max);
                } else if (Math.Sqrt(logPositiveResidueMeasurements.Count) < 100) {
                    var numberOfBins = BMath.Ceiling(Math.Sqrt(logPositiveResidueMeasurements.Count));
                    record.LogPositiveResiduesBins = logPositiveResidueMeasurements
                        .MakeHistogramBins(numberOfBins, min, max);
                } else {
                    record.LogPositiveResiduesBins = logPositiveResidueMeasurements.MakeHistogramBins(100, min, max);
                }
            }
            return record;
        }
    }
}

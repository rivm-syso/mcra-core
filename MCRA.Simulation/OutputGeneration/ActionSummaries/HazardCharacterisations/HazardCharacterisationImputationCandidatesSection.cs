using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Utils.Statistics;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationImputationCandidatesSection : SummarySection {

        public List<HazardCharacterisationImputationRecord> Records { get; set; }

        public double[] PercentilesCramerClassI { get; set; }
        public double[] PercentilesCramerClassII { get; set; }
        public double[] PercentilesCramerClassIII { get; set; }
        public double[] PercentilesAll { get; set; }

        public double HarmonicMeanCramerClassI { get; set; }
        public double HarmonicMeanCramerClassII { get; set; }
        public double HarmonicMeanCramerClassIII { get; set; }
        public double HarmonicMeanAll { get; set; }

        public void Summarize(
            Effect effect,
            ICollection<IHazardCharacterisationModel> imputationRecords
        ) {
            var percentages = new double[] { 50, 95 };
            var valuesCramerClassI = imputationRecords.Where(r => r.Substance.CramerClass == 1).Select(r => r.Value).ToList();
            if (valuesCramerClassI.Any()) {
                PercentilesCramerClassI = valuesCramerClassI.Percentiles(percentages);
                HarmonicMeanCramerClassI = 1D / valuesCramerClassI.Select(r => 1D / r).Average();
            }
            var valuesCramerClassII = imputationRecords.Where(r => r.Substance.CramerClass == 2).Select(r => r.Value).ToList();
            if (valuesCramerClassII.Any()) {
                PercentilesCramerClassII = valuesCramerClassII.Percentiles(percentages);
                HarmonicMeanCramerClassII = 1D / valuesCramerClassII.Select(r => 1D / r).Average();
            }
            var valuesCramerClassIII = imputationRecords.Where(r => r.Substance.CramerClass == 3).Select(r => r.Value).ToList();
            if (valuesCramerClassIII.Any()) {
                PercentilesCramerClassIII = valuesCramerClassIII.Percentiles(percentages);
                HarmonicMeanCramerClassIII = 1D / valuesCramerClassIII.Select(r => 1D / r).Average();
            }
            var valuesAll = imputationRecords.Select(r => r.Value).ToList();
            if (valuesAll.Any()) {
                PercentilesAll = valuesAll.Percentages(percentages);
                HarmonicMeanAll = 1D / valuesAll.Select(r => 1D / r).Average();
            }
            Records = imputationRecords
                .Select(model => {
                    var nominalIntraSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN;
                    var targetDose = model?.Value ?? double.NaN;
                    return new HazardCharacterisationImputationRecord() {
                        CompoundName = model.Substance.Name,
                        CompoundCode = model.Substance.Code,
                        CramerClass = model.Substance?.CramerClass,
                        EffectName = effect?.Name,
                        EffectCode = effect?.Code,
                        HazardCharacterisation = targetDose,
                        GeometricStandardDeviation = model?.GeometricStandardDeviation ?? double.NaN,
                        SystemHazardCharacterisation = model?.TestSystemHazardCharacterisation.HazardDose ?? double.NaN,
                        SystemDoseUnit = model?.TestSystemHazardCharacterisation?.DoseUnit != null
                            ? model?.TestSystemHazardCharacterisation?.DoseUnit.GetShortDisplayName()
                            : null,
                        Species = model?.TestSystemHazardCharacterisation?.Species,
                        Organ = model?.TestSystemHazardCharacterisation?.Organ,
                        ExposureRoute = model.TestSystemHazardCharacterisation != null
                            && model.TestSystemHazardCharacterisation.ExposureRoute != ExposureRoute.Undefined
                            ? model.TestSystemHazardCharacterisation.ExposureRoute.GetShortDisplayName()
                            : null,
                        PotencyOrigin = model?.PotencyOrigin.GetDisplayName(),
                        UnitConversionFactor = model?.TestSystemHazardCharacterisation?.TargetUnitAlignmentFactor ?? double.NaN,
                        ExpressionTypeConversionFactor = model?.TestSystemHazardCharacterisation?.ExpressionTypeConversionFactor ?? double.NaN,
                        NominalInterSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.InterSystemConversionFactor ?? double.NaN,
                        NominalIntraSpeciesConversionFactor = model?.TestSystemHazardCharacterisation?.IntraSystemConversionFactor ?? double.NaN,
                        NominalKineticConversionFactor = model?.TestSystemHazardCharacterisation?.KineticConversionFactor ?? double.NaN,
                    };
                })
                .OrderBy(r => r.EffectName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.EffectCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

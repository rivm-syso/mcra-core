using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {

    /// <summary>
    /// HBM concentrations standardization calculator for blood to total lipid content based 
    /// on enzymatic summation analysis.
    /// </summary>
    public class LipidEnzymaticCorrectionCalculator : IBloodCorrectionCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            TimeScaleUnit timeScaleUnit,
            Dictionary<TargetUnit, HashSet<Compound>> substanceTargetUnits            
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var totalLipidAlignmentFactor = getAlignmentFactor(targetUnit.GetConcentrationMassUnit(), ConcentrationUnit.mgPerdL);
                if (sampleCollection.SamplingMethod.IsBlood) {
                    var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.LipidEnz / totalLipidAlignmentFactor,
                                    targetUnit,
                                    sample.SamplingMethod.BiologicalMatrix,
                                    timeScaleUnit,
                                    substanceTargetUnits
                                 ))
                                .ToDictionary(c => c.MeasuredSubstance);
                            return new HumanMonitoringSampleSubstanceRecord() {
                                HumanMonitoringSampleSubstances = sampleCompounds,
                                HumanMonitoringSample = sample.HumanMonitoringSample
                            };
                        })
                        .ToList();
                    result.Add(new HumanMonitoringSampleSubstanceCollection(
                        sampleCollection.SamplingMethod,
                        newSampleSubstanceRecords,
                        sampleCollection.TriglycConcentrationUnit,
                        sampleCollection.CholestConcentrationUnit,
                        sampleCollection.LipidConcentrationUnit,
                        sampleCollection.CreatConcentrationUnit
                    )
                    );
                } else {
                    result.Add(sampleCollection);
                }
            }
            return result;
        }

        private SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           double? lipidEnz,
           ConcentrationUnit concentrationUnit,
           BiologicalMatrix biologicalMatrix,
           TimeScaleUnit timeScaleUnit,
           Dictionary<TargetUnit, HashSet<Compound>> substanceTargetUnits
        ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (sampleSubstance.MeasuredSubstance.IsLipidSoluble != true) {
                return sampleSubstance;
            }
            var clone = sampleSubstance.Clone();
            if (lipidEnz.HasValue && lipidEnz.Value != 0D) {
                clone.Residue = sampleSubstance.Residue / lipidEnz.Value;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            substanceTargetUnits.RemoveWhere(biologicalMatrix, s => s.Code == sampleSubstance.ActiveSubstance.Code);
            substanceTargetUnits.NewOrAdd(new TargetUnit(concentrationUnit.GetSubstanceAmountUnit(), ConcentrationMassUnit.Grams, timeScaleUnit, biologicalMatrix, ExpressionType.Lipids),
                                          sampleSubstance.ActiveSubstance);
           
            return clone;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid).
        /// </summary>
        private double getAlignmentFactor(ConcentrationMassUnit targetMassUnit, ConcentrationUnit unit) {
            var massUnit = unit.GetConcentrationMassUnit();
            var amountUnit = unit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(targetMassUnit);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Grams, 1);
            var multiplier = multiplier1 / multiplier2;
            return multiplier;
        }
    }
}
